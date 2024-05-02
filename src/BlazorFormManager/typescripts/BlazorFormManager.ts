import { logDebug, logError, logInfo, logWarning } from "./ConsoleLogger";
import { addEventListener, filterKeys, removeEventListener } from "./DomEventManager";
import { DragDropManager } from "./DragDropManager";
import { FileReaderManager } from "./FileReaderManager";
import { ReCAPTCHA } from "./ReCAPTCHA";
import { QuillEditor } from "./QuillEditor";
import { ChartjsHelper } from "./ChartjsHelper";
import { AssemblyName, Forms } from "./Shared";
import { containsFiles, formDataKeys, formDataMerge, populateDictionary, removeFileList, _isDictionary, _isFunction, _isObject, _isString, insertScripts, insertStyles } from "./Utils";
import { SimpleEvent } from "./SimpleEvent";

const global = globalThis;
const dotNet: DotNetStaticReference = global.DotNet;
const XHRSTATE = { UNSENT: 0, OPENED: 1, HEADERS_RECEIVED: 2, LOADING: 3, DONE: 4 };

const _resolvedPromise: Promise<boolean> = new Promise((resolve, _) => resolve(false));
const SESSION_FORMDATA_KEY = 'BlazorFormManager.Session.FormData';

/** Represents an object that manages forms' interactions and submissions. */
export class BlazorFormManager implements IBlazorFormManager, FormManagerInteropRef {

    private reCaptcha: ReCAPTCHA;
    private fileManager: FileReaderManager;
    private dragdropManager: DragDropManager;
    private static _supportsAJAXwithUpload = BlazorFormManager.supportsAjaxUploadWithProgress();
    private processedFileStorage: IDictionary<ProcessedFileConfig> = {};
    private formEvents: SimpleEvent<string>;
    private _enhancedLoadEventEnabled = false;

    /** Construct a new instance of the form manager class. */
    constructor() {
        this.fileManager = new FileReaderManager(this);
        this.dragdropManager = new DragDropManager(this.fileManager, this);
    }

    /**
     * Register a form submit manager.
     * @param options The options used to initialize the form manager.
     */
    init(options: FormManagerOptions) {
        if (!this.updateOptions(options))
            return BlazorFormManager.showOptionsUsage(false);

        const { dotNetObjectReference, formId } = options;
        logDebug(formId, "Initializing with options:", options);

        if (!this.validateFormAttributes(formId))
            return false;

        if (!dotNetObjectReference) {
            logWarning(formId, "Invokable object instance reference not received. " +
                "This is the preferred way of invoking.NET methods from within " +
                "Blazor components.");
        }

        if (this.registerSubmitHandler(options)) {
            logInfo(formId, "BlazorFormManager initialized successfully!");
            return true;
        }
        return false;
    }

    /**
     * Destroy all resources used by formId.
     * @param formId The form identifier.
     */
    destroy(formId: string) {
        if (Forms[formId]) {
            logDebug(formId, "Deleting form manager options...");
            delete Forms[formId];
        }
    }

    /**
     * Update existing form options with the new one.
     * @param options The new form options.
     */
    updateOptions(options: FormManagerOptions) {
        // fail early
        if (!_isObject(options))
            return BlazorFormManager.showOptionsUsage(true);

        const { formId } = options;

        if (!Forms[formId]) {
            Forms[formId] = options;
            logDebug(formId, "Script options stored.", options);
        } else {
            const storedOptions = Forms[formId];
            let count = 0;
            for (const key in options) {
                if (options.hasOwnProperty(key)) {
                    storedOptions[key] = options[key];
                    count++;
                }
            }
            logDebug(formId, `${count} script properties updated.`);
        }

        return true;
    }

    /**
     * Submit the form identified by the given parameter.
     * @param formId The identifier of the form to submit.
     */
    submitForm(formId: string) {
        logDebug(formId, "Form submit requested.", formId);

        const form = document.getElementById(formId) as HTMLFormElement;

        if (!form) {
            logError(formId, `Form #${formId} not defined`);
            return false;
        }

        if (form.onsubmit) {
            form.onsubmit(new SubmitEvent('submit'));
            logInfo(formId, "Form submitted via 'BlazorFormManager.submitForm'.")
            return true;
        }

        logError(formId, `'onsubmit' event handler not defined for form #${formId}.`);
        return false;
    }

    /**
     * Dynamically initialize MDBootstrap 5 inputs matching the specified CSS query selector.
     * @param options Initialization options.
     */
    initMdbInput(options: MDBootstrapOptions) {
        const { formId, selector: inputSelector, scoped } = options;
        let selector = inputSelector || '.form-outline';
        if (scoped) selector = `#${formId} ${selector}`;

        logDebug(formId, `Initializing "MDBootstrap 5" inputs with query selector '${selector}'...`);

        let query = document.querySelectorAll(selector);

        if (query.length) {
            const win = window as any;
            if (win.mdb && win.mdb.Input) {
                logDebug(formId, `"MDBootstrap 5" UMD module: ${query.length} input.s found.`);
                query.forEach(formOutline => {
                    if (!formOutline.classList.contains('select')) {
                        new win.mdb.Input(formOutline).init();
                    } else {
                        //new win.mdb.Dropdown(formOutline, {});
                    }
                });
            } else if (win.Input) {
                logDebug(formId, `"MDBootstrap 5" ES module: ${query.length} input.s found.`);
                query.forEach(formOutline => {
                    if (!formOutline.classList.contains('select')) {
                        new win.Input(formOutline).init();
                    } else {
                    }
                });
            } else {
                logDebug(formId, `No "MDBootstrap 5" module: ${query.length} input.s found.`);
            }
        } else {
            logDebug(formId, "No input found matching the specified CSS query selector.");
        }

        selector = scoped ? `#${formId} .form-floating>label` : '.form-floating>label';
        query = document.querySelectorAll(selector);

        if (query.length) {
            logDebug(formId, `Found ${query.length} input.s with query selector '${selector}'.`);

            /** Mimick form-outline effect for 'select' elements. */
            const maybeSetActive = (select: HTMLSelectElement, label: HTMLLabelElement, focused = false) => {
                const selectIsNumber = label.parentElement.classList.contains('number');
                let active = focused;
                if (!focused) {
                    const invalid =
                        selectIsNumber && (select.value === '0' || select.value === '') ||
                        (!selectIsNumber && select.value === '');
                    active = !invalid;
                }
                active ? select.classList.add('active') : select.classList.remove('active');
            };

            query.forEach((label: HTMLLabelElement) => {
                const { previousElementSibling: select } = label;
                if (select instanceof HTMLSelectElement) {
                    const { border: initialBorder, color: initialColor } = select.style;

                    // the free version of MDBootstrap 5 doesn't allow specifying 
                    // '.form-outline' on a 'select' element when initializing inputs
                    label.parentElement.classList.replace('form-floating', 'form-outline');

                    maybeSetActive(select, label);

                    select.addEventListener('focus', () => {
                        maybeSetActive(select, label, true);
                        label.style.color = 'var(--mdb-picker-header-bg)';
                        select.style.border = '2px solid var(--mdb-picker-header-bg)';
                    });

                    select.addEventListener('blur', () => {
                        maybeSetActive(select, label);
                        label.style.color = initialColor;
                        select.style.border = initialBorder;
                    });
                }
            });
        }
    }

    /**
     * Sets the value of the pair identified by key to value, creating 
     * a new key/value pair if none existed for key previously.
     * @param key The key of the value to set.
     * @param value The value to set.
     */
    localStorageSetItem(key: string, value: string) {
        localStorage.setItem(key, value);
    }

    /**
     * Removes the key/value pair with the given key from the list associated 
     * with the object, if a key/value pair with the given key exists.
     * @param key The key of the value to remove.
     */
    localStorageRemoveItem(key: string) {
        localStorage.removeItem(key);
    }

    /**
     * Returns the current value associated with the given key, or null if 
     * the given key does not exist in the list associated with the object.
     * @param key The key of the value to retrieve.
     */
    localStorageGetItem(key: string) {
        return localStorage.getItem(key);
    }

    /**
     * Register an AJAX handler for the submission of a form.
     * @param options An configuration object used to register the form submit handler.
     */
    registerSubmitHandler(options: FormManagerOptions) {
        const {
            formId,
            onBeforeSubmit,
            onBeforeSend,
            onSendFailed,
            onSendSucceeded,
            onReCaptchaActivity,
            requireModel,
            reCaptcha,
            enhancedLoad,
            onEnhancedLoad,
            sessionStorageKey,
        } = options;

        const _this = this;
        const recaptcha = _this.initReCaptcha(formId, reCaptcha, onReCaptchaActivity);

        if (enhancedLoad && _isString(onEnhancedLoad)) {
            this._enableEnhancedLoad(formId, true, onEnhancedLoad);
        }

        return this.handleFormSubmission({
            formId,
            requireModel,
            reCaptcha,
            getFormData: () => this.getFormData(formId),
            beforeSubmit: async function () {
                let cancel = false;
                if (_isString(onBeforeSubmit))
                    cancel = await _this.invokeDotNet(formId, onBeforeSubmit);
                return cancel;
            },
            beforeSend: async function () {
                let cancel = false;
                if (_isString(onBeforeSend))
                    cancel = await _this.invokeDotNet(formId, onBeforeSend,
                        getXhrResult.call(this, true)
                    );
                return cancel;
            },
            done: function () {
                logInfo(formId, "Form data successfully uploaded.", this);

                if (_isString(onSendSucceeded))
                    _this.invokeDotNet(formId, onSendSucceeded,
                        getXhrResult.call(this, false)
                    );

                // eventually free up the processed file storage for this form
                _this.fileManager.deleteProcessedFileList({ formId });

                _this.raiseFormSubmitted(formId);
            },
            fail: function (error) {
                logError(formId, "Form upload failed.", this);

                if (_isString(onSendFailed))
                    _this.invokeDotNet(formId, onSendFailed,
                        getXhrResult.call(this, false, error)
                    );

                recaptcha && recaptcha.reset(formId, reCaptcha);
            }
        });

        function getXhrResult(excludeHeaders: boolean, error?: string): XhrResult {
            const extraProperties = { error };
            const xhr: XMLHttpRequest = this;

            // these properties may throw an "InvalidStateError" DOMException
            let responseText: string, responseType: string, responseXML: string;

            // so we safely access them
            try { responseText = xhr.responseText || ''; } catch { responseText = null; }
            try { responseType = xhr.responseType || ''; } catch { responseType = null; }
            try { responseXML = xhr.responseXML && xhr.responseXML.textContent || null; } catch { responseXML = null; }

            return {
                response: xhr.response || null,
                responseHeaders: excludeHeaders ? null : xhr.getAllResponseHeaders(),
                responseText,
                responseType,
                responseXML,
                status: xhr.status,
                statusText: xhr.statusText || "",
                timeout: xhr.timeout,
                extraProperties,
            };
        }
    }

    async getFormData(formId: string) {
        const { onGetModel, requireModel } = Forms[formId] || {};

        // custom routine for retrieving form data defined with models
        let model;

        if (_isString(onGetModel)) {
            model = await this.invokeDotNet(formId, onGetModel);
            logDebug(formId, "Model", model);
        }

        if (model === null) {
            if (requireModel) {
                logError(formId, "A model is required.");
            } else {
                logDebug(formId, "Model not defined. Caller will collect FormData.");
            }
            return { hasFiles: false, formData: undefined, json: '' };
        }

        // collect existing inputs from the form
        const form = document.getElementById(formId) as HTMLFormElement;
        const enctype = form.getAttribute("enctype") || 'multipart/form-data';

        // check if the form contains any files;
        // determines whether file upload progress events are monitored
        const hasFiles = containsFiles(form);

        if (enctype.toLowerCase().indexOf('json') > -1) {
            if (hasFiles)
                logWarning(formId, "The form contains files which cannot be sent using the JSON format.");
            else
                logDebug(formId, "Collecting form model data as JSON...");
            // send as JSON
            const json = BlazorFormManager.objectFromEntries(new FormData(form), model);
            return { hasFiles, formData: undefined, json };
        } else {
            logDebug(formId, "Collecting form model data...");

            const formData = new FormData();

            // add additional form data values using the model...
            this.collectModelData(model, formData);
            formDataMerge(formData, new FormData(form));

            return { hasFiles, formData, json: '' };
        }
    }

    async storeFormData(formId: string, sessionStorageKey: string) {
        const { formData, json } = await this.getFormData(formId);
        const itemKey = `${SESSION_FORMDATA_KEY}:${sessionStorageKey}`;
        if (json) {
            sessionStorage.setItem(itemKey, json);
            return json;
        }
        else if (formData) {
            const data = BlazorFormManager.objectFromEntries(formData);
            sessionStorage.setItem(itemKey, data);
            return data;
        }
    }

    retrieveFormData(sessionStorageKey: string) {
        const itemKey = `${SESSION_FORMDATA_KEY}:${sessionStorageKey}`;
        return sessionStorage.getItem(itemKey);
    }

    /**
     * Set the log level.
     * @param config The configuration options.
     */
    setLogLevel(config: FormManagerBaseOptions) {
        const { formId, logLevel: level } = config;
        const options = Forms[formId];
        if (!options) {
            logError(formId, `The form #${formId} has not been initialized yet.`);
        } else if (options.logLevel !== level) {
            if (typeof level === "number") {
                const validLevel = 0 | level; // force the number to be an integer
                if (validLevel > -1 && validLevel < 5) {
                    options.logLevel = validLevel;
                    logInfo(formId, "Log level changed to", validLevel);
                    return true;
                }
            }
            logError(formId, "Log level must be an integer between 0 and 4 inclusive.");
        }
        return false;
    }

    /**
     * Handle all aspects of an asynchronous form submission.
     * @param options The configuration object to use.
     */
    handleFormSubmission(options: FormSubmissionOptions) {
        if (!_isObject(options)) return false;

        const { formId, requireModel, reCaptcha } = options;

        if (!_isString(formId)) {
            logError(formId, "No form identified!");
            return false;
        }

        const form = document.getElementById(formId) as HTMLFormElement;

        if (!form) {
            logError(formId, `Form with id #${formId} not found!`);
            return false;
        }

        form.onsubmit = async () => {
            logDebug(formId, "Submitting form...");
            let cancel = false;
            const { beforeSubmit } = options;

            if (_isFunction(beforeSubmit)) cancel = await beforeSubmit.call(form);

            if (cancel) {
                logInfo(formId, "Form submission was cancelled.");
                return false;
            }

            // With nowadays (modern) browsers, this is VERY unlikely, but just in case...
            if (!BlazorFormManager._supportsAJAXwithUpload) {
                logWarning(formId, "AJAX upload with progress report not supported.");

                cancel = await this.raiseAjaxUploadWithProgressNotSupported(formId);

                if (cancel) {
                    // do not allow full page refresh
                    logInfo(formId, "Blocked submitting form with full-page refresh.");
                    return false;
                }

                // allow normal form submission (post back with full page refresh)
                return true;
            }

            const { beforeSend, done, fail, getFormData } = options;
            let { formData, hasFiles, json } = await getFormData();
            const isJson = !!json;

            if (!formData && !isJson) {
                if (requireModel) {
                    logError(formId, "Form submission cancelled because a model is required to continue.");
                    return false;
                } else {
                    formData = new FormData(form);
                    hasFiles = containsFiles(form);
                }
            }

            if (!isJson && this.clearFilesAppendProcessed(formId, formData))
                hasFiles = true;

            const xhr = new XMLHttpRequest();

            xhr.onreadystatechange = function () {
                if (this.readyState === XHRSTATE.DONE) {
                    // any status code between 200 and 299 inclusive is success
                    if (this.status > 199 && this.status < 300) {
                        if (_isFunction(done))
                            done.call(this);
                    } else if (_isFunction(fail)) {
                        fail.call(this);
                    }
                }
            };

            this.addXHRUploadEventListeners(formId, xhr, hasFiles);

            const url = form.getAttribute("action") || global.location.href;
            const method = (form.getAttribute("method") || "POST").toUpperCase();

            logDebug(formId, "Form action URL and method are:", url, method);

            xhr.open(method, url, true);

            let headersSet = false;

            if (_isFunction(beforeSend)) {
                const xhrResult = await beforeSend.call(xhr);
                logDebug(formId, "beforeSend was called:", xhrResult);

                if (_isObject(xhrResult)) {
                    if (xhrResult.cancel) {
                        logDebug(formId, "XMLHttpRequest.send was cancelled.");
                        return false;
                    }

                    logDebug(formId, "Trying to set XMLHttpRequest (XHR) properties...");
                    try {
                        headersSet = this.setRequestHeaders(formId, xhr, xhrResult.requestHeaders);
                        xhr.withCredentials = xhrResult.withCredentials;
                    } catch (e) {
                        logError(formId, "Error setting XHR properties.", e);
                        if (_isFunction(fail)) fail.call(xhr, e.message);
                        return false;
                    }
                }
            }

            if (!headersSet && Forms[formId])
                this.setRequestHeaders(formId, xhr, Forms[formId].requestHeaders);

            if (isJson) {
                this.setRequestHeaders(formId, xhr, { 'content-type': 'application/json', accept: 'application/json; text/plain' });
            }

            logDebug(formId, "Adjusting bad form data keys.");
            const objData = this.adjustBadFormDataKeys(formData || json);

            if (this.reCaptcha) {
                logDebug(formId, "Submitting form using reCAPTCHA.");
                this.reCaptcha.submitForm(formId, xhr, objData, reCaptcha);
            } else {
                logDebug(formId, "Submitting form using XMLHttpRequest.");
                xhr.send(objData);
            }

            return false; // To avoid actual submission of the form
        }
        return true;
    }


    deleteProcessedFileList(options: { formId: string; inputId: string; }) {
        return this.fileManager.deleteProcessedFileList(options);
    }

    dragDropEnable(options: DragDropEnableOptions) {
        return this.dragdropManager.enable(options);
    }

    dragDropDisable(options: DragDropTargetOptions) {
        return this.dragdropManager.disable(options);
    }

    dragDropRemoveFileList(options: DragDropTargetOptions) {
        return removeFileList(options);
    }

    dragDropInputFilesOnTarget(options: DragDropInputFilesOptions) {
        return this.dragdropManager.dragDropInputFilesOnTarget(options);
    }

    filterKeys(options: FilterKeysOptions) {
        return filterKeys(options);
    }

    addEventListener(targetId: string, eventType: string, callback: string) {
        return addEventListener(targetId, eventType, callback);
    }

    /**
     * Remove an event listener from a target element identified by 'targetId'.
     * @param targetId The identifier of the target element.
     * @param eventType The type of event to remove the listener for.
     */
    removeEventListener(targetId: string, eventType: string) {
        return removeEventListener(targetId, eventType);
    }

    /**
     * Set request headers on the specified XMLHttpRequest object.
     * @param formId The form identifier.
     * @param xhr The XMLHttpRequest object.
     * @param headers A collection of key=value pairs.
     */
    setRequestHeaders(formId: string, xhr: XMLHttpRequest, headers: IDictionary<string>) {
        if (!_isDictionary(headers)) return false;

        logDebug(formId, "Setting request headers...");

        for (const name in headers) {
            if (headers.hasOwnProperty(name)) {
                const value = headers[name];
                logDebug(formId, "Header:", { name, value });
                xhr.setRequestHeader(name, value);
            }
        }

        return true;
    }

    /** Return the dictionary of registered form manager options. */
    getForms() {
        return Forms;
    }

    static supportsImageUtil() {
        // before checking for the ImageUtility class, which uses the 
        // <canvas> element, make sure first that the device supports canvas
        return !!window.CanvasRenderingContext2D;
    }

    /**
     * Invoke dotnet method 'onAjaxUploadWithProgressNotSupported'.
     * @param formId The form identifier.
     */
    async raiseAjaxUploadWithProgressNotSupported(formId: string) {
        let cancel = false;
        if (Forms[formId]) {
            const { onAjaxUploadWithProgressNotSupported } = Forms[formId];

            if (_isString(onAjaxUploadWithProgressNotSupported)) {
                const navigatorInfo = {};
                populateDictionary(navigatorInfo, global.navigator);
                logDebug(formId, "Navigator properties collected", navigatorInfo);

                cancel = await this.invokeDotNet(formId, onAjaxUploadWithProgressNotSupported,
                    { extraProperties: navigatorInfo }
                );
            }
        }
        return cancel;
    }

    /**
     * Read input files using the specified options.
     * @param options
     */
    readInputFiles(options: FileReaderOptions) {
        return this.fileManager.readInputFiles(options, processedFiles => {
            const { formId, inputId, inputName } = options;
            logDebug(formId, "Received processed files.");

            if (processedFiles && processedFiles.length > 0) {
                logDebug(formId, "Storing processed files...");
                // store files that have been processed so that they can be 
                // used later to validate when the form is being submitted
                let config = this.processedFileStorage[formId];

                if (!config) {
                    // create the configuration and store it
                    config = {
                        inputs: []
                    }
                    this.processedFileStorage[formId] = config;
                }

                const input = config.inputs.find(obj => obj.id === inputId);

                // update the configuration
                if (!input) {
                    config.inputs.push({
                        id: inputId,
                        name: inputName,
                        files: processedFiles
                    });
                    logDebug(formId, "Created processed files configuration.");
                } else {
                    input.files = processedFiles;
                    logDebug(formId, "Updated processed files configuration.");
                }
            } else {
                logDebug(formId, "No files processed!");
            }
        });
    }

    /**
     * Invoke a .NET method.
     * @param formId The form identifier.
     * @param method The .NET method to invoke.
     * @param arg The argument to the .NET method to invoke.
     */
    invokeDotNet<T>(formId: string, method: string, arg?: any): Promise<T | boolean> {
        if (arg === undefined) arg = null;
        const { dotNetObjectReference: obj } = Forms[formId] || {};

        if (obj) {
            //logDebug(formId, `Invoking .NET method "${method}" using object instance.`, arg);
            return obj.invokeMethodAsync<T>(method, arg);
        } else if (dotNet && AssemblyName) {
            //logDebug(formId, `Invoking .NET method "${method}" using static type`, arg);
            return dotNet.invokeMethodAsync<T>(AssemblyName, method, arg);
        }
        logWarning(formId, 'No DotNetObjectReference nor static .NET interop reference defined!');
        return _resolvedPromise;
    }

    /**
     * Reset previously created reCAPTCHA widgets for the specfified form.
     * @param formId The form identifier.
     * @param options The reCAPTCHA options.
     */
    resetRecaptcha(formId: string, options?: ReCaptchaOptions) {
        logDebug(formId, 'resetting reCAPTCHA', options);
        const recap = this.reCaptcha;
        recap && recap.reset(formId, options);
    }

    /**
     * 
     * Reconfigure the reCAPTCHA for the specified form identifier.
     * @param formId The form identifier.
     * @param options The configuration options.
     */
    reConfigureRecaptcha(formId: string, options?: ReCaptchaOptions) {
        logDebug(formId, 'Reconfiguring reCAPTCHA', options);
        const recap = this.reCaptcha;
        recap && recap.configure(formId, options, true);
    }

    /**
     * Uniquely insert one or more CSS styles into the DOM.
     * @param formId The form identifier.
     * @param styles A string or array of styles to insert.
     */
    insertDomStyles(formId: string, styles: string | string[]) {
        return insertStyles(styles, null, formId);
    }

    /**
     * Uniquely insert one or more scripts into the DOM.
     * @param formId The form identifier.
     * @param scripts A string or array of scripts to insert.
     * @param isAsync Sets the script's async property.
     * @param isDeferred Sets the scripts defer property.
     */
    insertDomScripts(formId: string, scripts: string | string[], isAsync?: boolean, isDeferred?: boolean) {
        return insertScripts(scripts, null, formId, isAsync, isDeferred);
    }

    initQRCode(formId: string, script?: string, onload?: (this: GlobalEventHandlers, ev: Event) => any) {
        logDebug(formId, 'Loading QRCode script...', script);
        script || (script = "https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.min.js");
        onload || (onload = () => logDebug(formId, 'Done.'));
        return insertScripts(script, onload, formId);
    }

    async generateQRCode(formId: string, selector: string, value: string) {
        let { qrCode } = Forms[formId] || (Forms[formId] = { formId });
        if (qrCode) {
            qrCode.delegate.clear();
            logDebug(formId, 'QRCode cleared.');
            if (value) {
                qrCode.delegate.makeCode(value);
                logDebug(formId, 'Generated new QRCode.');
            }
            return true;
        } else if (value) {
            const elm = document.querySelector(selector);
            if (elm) {
                let QRCode = window["QRCode"];
                const loadQRCode = () => {
                    qrCode = { delegate: new QRCode(elm, value), selector };
                    Forms[formId].qrCode = qrCode;
                    logDebug(formId, `QRCode generated successfully for text ${value}.`);
                }
                if (!QRCode) {
                    logDebug(formId, 'QRCode not initialized, loading default script...');
                    const success = await this.initQRCode(formId);
                    if (success) {
                        logDebug(formId, 'Done.');
                        QRCode = window["QRCode"];
                        loadQRCode();
                    } else {
                        logError(formId, 'Could not initialize the default QRCode script.');
                    }
                } else {
                    loadQRCode();
                }

                return true;
            }
            else {
                logError(formId, `generateQRCode: No DOM element matches the selector '${selector}'.`);
            }
        }
        return false;
    }

    destroyQrCode(formId: string) {
        const { qrCode } = Forms[formId];
        if (qrCode) {
            const { delegate, selector } = qrCode;
            delegate.clear();
            delete Forms[formId].qrCode;
            logDebug(formId, `The QRCode identified by ${selector} has been destroyed!`);
        }
    }

    /** Determines whether an environment supports asynchronous form submissions with file upload progress events. */
    static supportsAjaxUploadWithProgress() {
        return supportsFileAPI() &&
            supportsAjaxUploadProgressEvents() &&
            supportsFormData();

        function supportsFileAPI() {
            var fi = document.createElement("INPUT") as HTMLInputElement;
            fi.type = "file";
            return "files" in fi;
        };

        function supportsAjaxUploadProgressEvents() {
            var xhr = "XMLHttpRequest" in globalThis && new XMLHttpRequest();
            return !!(xhr && ("upload" in xhr) && ("onprogress" in xhr.upload));
        };

        function supportsFormData() {
            return !!global.FormData;
        }
    }

    private validateFormAttributes(formId: string) {
        const frm = document.getElementById(formId);
        if (!frm) {
            logWarning(formId, `The form element identified by '${formId}' does not exist in the DOM.`);
            return false;
        }
        return true;
    }

    private addXHRUploadEventListeners(formId: string, xhr: XMLHttpRequest, hasFiles: boolean) {
        const { onUploadChanged } = Forms[formId];
        if (!_isString(onUploadChanged)) return;
        logDebug(formId, "Setting up upload event handlers...");
        this.fileManager.addFileReaderOrUploadEventListeners(formId, xhr, xhr.upload, onUploadChanged, hasFiles, null);
    }

    private initReCaptcha(formId: string, reCaptcha: ReCaptchaOptions, onReCaptchaActivity?: string) {
        if (_isObject(reCaptcha)) {
            let recap = this.reCaptcha;
            if (!recap) {
                recap = new ReCAPTCHA();
                recap.activity.subscribe(activity => {
                    this.invokeDotNet(activity.formId, onReCaptchaActivity, activity);
                });
                this.reCaptcha = recap;
            }
            recap.configure(formId, reCaptcha);
        }
        return this.reCaptcha;
    }

    private static showOptionsUsage(isError: boolean) {
        const optionalNameOfMethodWhen = "string (optional): The name of the .NET method to invoke when ";
        const options = {
            formId: "string (required): The unique identifier of the form to submit.",

            requireModel: "boolean (optional): Gets or sets a value that indicates whether specifying a " +
                "non-null reference model is required when the 'onGetModel' event is invoked. If this value is " +
                "true and no valid model is provided after that event, the form submission will be cancelled.",

            enhancedLoad: "boolean (optional): Gets or sets a value that indicates whether to react to a Blazor Server App's 'enhanceload' event. This parameter allows resetting up specific aspects of the form, such as reinitializing the reCaptcha options.",

            onEnhancedLoad: optionalNameOfMethodWhen + "a Blazor Server App's 'enhancedLoad' event occurs.",

            assembly: "string (optional): The name of the .NET assembly on which " +
                "to invoke static methods. Required if any of the 'onXXX' static " +
                "method names are set.",

            logLevel: "number (optional): The level of details when logging messages " +
                "to the console. Supported values are: 0 (none), 1 (information), " +
                "2 (warning), 3 (error), 4 (debug). The default is 3.",

            requestHeaders: "object (optional): A dictionary of key-value pairs to set on an instance of XMLHttpRequest.",

            reCaptcha: "object (optional): An object that encapsulates configuration settings for Google's reCAPTCHA technology.",

            onGetModel: optionalNameOfMethodWhen + "retrieving the form model.",
            onBeforeSubmit: optionalNameOfMethodWhen + "submitting the form. This event can be cancelled.",
            onBeforeSend: optionalNameOfMethodWhen + "sending the HTTP request. This event can be cancelled.",
            onSendFailed: optionalNameOfMethodWhen + "the HTTP request fails.",
            onSendSucceeded: optionalNameOfMethodWhen + "the HTTP request completes successfully.",
            onUploadChanged: optionalNameOfMethodWhen + "an upload operation occurs.",
            onFileReaderChanged: optionalNameOfMethodWhen + "a change occurs in a file reading operation.",
            onFileReaderResult: optionalNameOfMethodWhen + "a file reading operation is completed successfully.",
            onReCaptchaActivity: optionalNameOfMethodWhen + "a Google reCAPTCHA activity is reported.",
            onAjaxUploadWithProgressNotSupported: optionalNameOfMethodWhen + "the browser does not support AJAX uploads with progress report.",
        };

        const message = "Initialization options must be a variation of the following format:";
        const formId: string = undefined;

        if (isError) logError(formId, message, options);
        else logInfo(formId, message, options);

        return false;
    }

    private static objectFromEntries(data: any, fallback?: undefined) {
        //const obj: any = Object;
        //const json = JSON.stringify(obj.fromEntries && obj.fromEntries(new FormData(form)) || model)
        //    .replace('"true"', 'true')
        //    .replace('"false"', 'false');

        const obj: any = Object;
        return JSON.stringify(obj.fromEntries && obj.fromEntries(data) || fallback || {})
            .replace('"true"', 'true')
            .replace('"false"', 'false');
    }

    /**
     * Collect all processed (approved) files before submitting the form.
     * @param formId The form identifier.
     */
    private collectFilesOnFormSubmit(formId: string) {
        return this.collectInputFiles(formId).concat(this.dragdropManager.collectFiles(formId));
    }

    /**
     * Collect all input files found in the specified form identifier.
     * @param formId The form identifier.
     */
    private collectInputFiles(formId: string) {
        const config = this.processedFileStorage[formId];

        if (!config) {
            logDebug(formId, "No processed files found for form #" + formId);
            return [];
        }

        const { inputs } = config;
        const collected: Array<{ name: string; value: File }> = [];

        for (let i = 0; i < inputs.length; i++) {
            const { name, files } = inputs[i];
            for (let k = 0; k < files.length; k++) {
                collected.push({
                    name,
                    value: files[k]
                });
            }
        }

        return collected;
    }

    /**
     * Make sure that only successfully processed files are submitted with the form data.
     * @param formId The form identifier.
     * @param formData The form data to check.
     */
    private clearFilesAppendProcessed(formId: string, formData: FormData) {
        if (!formId || !formData) {
            logError(formId, "formId and formData parameters must be set when validating processed files.");
            return false;
        }

        logDebug(formId, "Validating processed files on form submit");

        // get processed files...
        const processedFiles = this.collectFilesOnFormSubmit(formId);
        const hasFiles = processedFiles.length > 0;

        if (hasFiles) {
            this.clearFormDataFiles(formId, formData);
        }

        // ... and append them
        processedFiles.forEach(({ name, value: file }) => {
            formData.append(name, file, file.name);
        });

        return hasFiles;
    }

    /**
     * Remove all files from the specified form data.
     * @param formId The form identifier.
     * @param formData The form data to clear.
     */
    private clearFormDataFiles(formId: string, formData: FormData) {
        if (!formData) return false;

        // first, grab the keys to avoid deleting 
        // entries from the formData while enumerating
        const keys = formDataKeys(formData);

        logDebug(formId, "Removing all files from form data; keys are: ", keys);

        let success = false;

        // delete all files
        for (let i = 0; i < keys.length; i++) {
            const entry = formData.get(keys[i]);
            if (entry instanceof File) {
                formData.delete(keys[i]);
                success = true;
            }
        }

        return success;
    }

    /**
     * Remove inappropriate data keys.
     * @param data The data to clean up.
     */
    private adjustBadFormDataKeys(data: FormData | string) {
        const RVT_KEY1 = '__RequestVerificationToken';
        const RVT_KEY2 = 'requestVerificationToken';

        if (data instanceof FormData) {
            const rvt1 = data.get(RVT_KEY1);
            const rvt2 = data.get(RVT_KEY2);

            if (!_isString(rvt1))
                data.set(RVT_KEY1, rvt2);

            data.delete(RVT_KEY2);
            return data;
        } else {
            const json = JSON.parse(data);
            const rvt1 = json[RVT_KEY1];
            const rvt2 = json[RVT_KEY2];

            if (!_isString(rvt1))
                json[RVT_KEY1] = rvt2;

            delete json[RVT_KEY2];
            return JSON.stringify(json);
        }
    }

    /**
     * Recursively append to the specified form data properties and values from the given model.
     * @param model The model from which to collect form data.
     * @param formData The FormData object to which collected values are appended.
     * @param path The current navigation path to a given property of the model.
     */
    private collectModelData(model, formData: FormData, path?: string) {
        if (_isObject(model) && !(model instanceof Date) && !(model instanceof File) && !(model instanceof Blob)) {
            Object.keys(model).forEach(key => {
                this.collectModelData(model[key], formData, path ? `${path}[${key}]` : key);
            });
        } else {
            const value = (model === null || model === undefined) ? null : model;
            if (model !== null) formData.append(path, value);
        }
    }

    private raiseFormSubmitted(formId: string) {
        logWarning(formId, 'raiseFormSubmitted not implemented!');
    }


    private _enableEnhancedLoad(formId: string, enabled: boolean, dotnetMethodName: string) {
        const Blazor: any = global['Blazor'];
        if (Blazor?.addEventListener) {
            if (enabled) {
                if (!this._enhancedLoadEventEnabled) {
                    Blazor.addEventListener('enhancedload', () => {
                        logDebug(formId, 'Blazor.enhancedload event occurred!');
                        // the method must be static
                        // dotNet.invokeMethodAsync(AssemblyName, dotnetMethodName, window.location.href);
                        setTimeout(async () => {
                            // if reCAPTCHA is required, force a full refresh to make it work correctly
                            if (this.reCaptcha?.requiresReCaptcha()) {
                                if (this.reCaptcha.getFirstOption()?.version === 'v3') {
                                    const { sessionStorageKey } = Forms[formId] || {};
                                    if (_isString(sessionStorageKey)) {
                                        const stored = await this.storeFormData(formId, sessionStorageKey);
                                        logDebug(formId, 'Storing form data', stored);
                                    }
                                    // reCAPTCHA version 3 doesn't tolerate dynamic script insertion
                                    // it should be safe to reload the entire page since
                                    // this routine is executed after an enhanced navigation
                                    window.location.reload();
                                } else {
                                    this.reCaptcha.configure(formId, null, true);
                                }
                            }
                        }, 500);
                    });
                    this._enhancedLoadEventEnabled = true;
                    logDebug(formId, 'Blazor enhancedload event enabled.');
                }
            } else {
                if (Blazor.removeEventListener) {
                    Blazor.removeEventListener('enhancedload');
                    logDebug(formId, 'Blazor enhancedload event disabled.');
                }
                this._enhancedLoadEventEnabled = false;
            }
        } else {
            logWarning(formId, '"enhanceload" event not supported: No global Blazor object found.');
        }
    }
}

(function () {
    const blazorFormManager = new BlazorFormManager();

    Object.defineProperty(global, 'BlazorFormManager', { value: blazorFormManager });

    // define immutable properties
    Object.defineProperty(blazorFormManager, 'QuillEditor', { value: QuillEditor });
    Object.defineProperty(blazorFormManager, 'Quill', { value: {} });
    Object.defineProperty(blazorFormManager['Quill'], 'create', {
        /**
        * Create a new QuillEditor instance.
        * @param {string} selector A DOM query selector.
        * @param {any} options The options.
        */
        value: function (selector, options) {
            var editor = new QuillEditor(selector, options);
            return editor;
        }
    });
    Object.defineProperty(blazorFormManager['Quill'], 'getInnerHTML', {
        value: function (editor: QuillEditor) {
            return editor.getInnerHTML();
        }
    });
    Object.defineProperty(blazorFormManager, 'ChartjsHelper', { value: ChartjsHelper });
})();
