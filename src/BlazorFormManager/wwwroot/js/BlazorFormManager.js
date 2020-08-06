/*
 Name:          BlazorFormManager
 Version:       1.0.2
 Author:        Abdourahamane Kaba
 Description:   Handle AJAX form data submission with zero or more files, and
                report back data upload activities to a .NET Blazor Component.
 License:       Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 Copyright:     © Karfamsoft. All rights reserved.
 */
(function (global) {
    const XHRSTATE = { UNSENT: 0, OPENED: 1, HEADERS_RECEIVED: 2, LOADING: 3, DONE: 4 };
    const CONSOLE_FUNC = { info: "info", warn: "warn", error: "error", log: "log" };
    const LOG_LEVEL = { none: 0, information: 1, warning: 2, error: 3, debug: 4 };
    const UPLOAD_EVENTS = { loadstart: 0, progress: 1, load: 2, error: 3, abort: 4, timeout: 5, loadend: 6 };
    const PRIMITIVES = ["string", "number", "bigint", "boolean"];

    const _dotnet = global.DotNet;
    const _document = global.document;
    const _supportsAJAXwithUpload = supportsAjaxUploadWithProgress();

    let _form;
    let _managerOptions;
    let _invokableInstanceRef;

    global.BlazorFormManager = {
        init: function (options, objInstanceRef) {
            _managerOptions = options;
            _invokableInstanceRef = objInstanceRef;
            logInfo("Initializing with options:", options);

            // fail early
            if (!_isObject(options) || !_isString(options.formId))
                return showOptionsUsage(true);

            if (!validateFormAttributes(options.formId)) {
                return false;
            }

            if (!!_invokableInstanceRef) {
                logInfo("Invokable object instance reference received.", _invokableInstanceRef);
            } else {
                logWarning("Invokable object instance reference not received. " +
                    "This is the preferred way of invoking.NET methods from " +
                    "withing Blazor components.");
            }

            registerSubmitHandler();
            logInfo("Initialized successfully!");
            return true;
        },
        setLogLevel: function (level) {
            if (!!_managerOptions) {
                if (_managerOptions.logLevel !== level) {
                    if (typeof level === "number") {
                        const validLevel = 0 | level; // force the number to be an integer
                        if (validLevel > -1 && validLevel < 5) {
                            _managerOptions.logLevel = validLevel;
                            logInfo("Log level changed to", validLevel);
                            return true;
                        }
                    }
                    logError("Log level must be an integer between 0 and 4 inclusive.");
                }
            } else {
                logError("Initialization has not been done yet.");
            }
            return false;
        },
        raiseAjaxUploadWithProgressNotSupported: function () {
            logDebug("raiseAjaxUploadWithProgressNotSupported() invoked in script.");
            raiseAjaxUploadWithProgressNotSupported();
        },
        submitForm: function () {
            logDebug("Form submit requested.");

            if (!_form) {
                logInfo("Form not defined");
                return false;
            }

            if (!!_form.onsubmit) {
                _form.onsubmit();
                logInfo("Form submitted via 'BlazorFormManager.submitForm'.")
                return true;
            }

            logDebug("Form's 'onsubmit' event handler not defined.");
            return false;
        },
        localStorageSetItem: function (key, value) {
            localStorage.setItem(key, value);
        },
        localStorageRemoveItem: function (key) {
            localStorage.removeItem(key);
        },
        localStorageGetItem: function (key) {
            return localStorage.getItem(key);
        },
        request: function (url, method, headers, credentials) {
            return fetchRequest(url, method, headers, credentials);
        }
    };

    /**
    * Register an AJAX handler for the submission of a form with a file.
    */
    function registerSubmitHandler() {
        const {
            formId,
            onGetModel,
            onBeforeSubmit,
            onBeforeSend,
            onSendFailed,
            onSendSucceeded,
        } = _managerOptions;

        handleFormSubmission({
            formId,
            getFormData: async () => {
                // custom routine for retrieving form data defined with models
                let model;

                if (_isString(onGetModel)) {
                    model = await invokeDotNet(onGetModel);
                    logDebug("Model", model);
                }

                if (!model) {
                    logDebug("Model not defined. FormData will be collected by caller.");
                    return {};
                }

                logDebug("Collecting form model data...");

                // collect existing inputs from the form
                const form = _document.getElementById(formId);
                const formData = new FormData(form);

                // add additional form data values using the model...
                collectModelData(model, formData);

                // check if the form contains any files
                const hasFiles = containsFiles(form);
                return { formData, hasFiles };
            },
            beforeSubmit: async () => {
                let cancel = false;
                if (_isString(onBeforeSubmit))
                    cancel = await invokeDotNet(onBeforeSubmit);
                return cancel;
            },
            beforeSend: async () => {
                let cancel = false;
                if (_isString(onBeforeSend))
                    cancel = await invokeDotNet(onBeforeSend,
                        getXhrResult.call(this, true)
                    );
                return cancel;
            },
            done: function () {
                logInfo("Form data successfully uploaded.");

                if (_isString(onSendSucceeded))
                    invokeDotNet(onSendSucceeded,
                        getXhrResult.call(this, false)
                    );
            },
            fail: function (error) {
                logError("Form upload failed.");
                logDebug("XHR", this);

                if (_isString(onSendFailed))
                    invokeDotNet(onSendFailed,
                        getXhrResult.call(this, false, error)
                    );
            }
        });

        function getXhrResult(excludeHeaders, error) {
            const extraProperties = !!error ? { error } : {};

            return {
                response: this.response || null,
                responseHeaders: excludeHeaders ? null : this.getAllResponseHeaders(),
                responseText: this.responseText || "",
                responseType: this.responseType || "",
                responseXML: this.responseXML || "",
                status: 0 | this.status,
                statusText: this.statusText || "",
                timeout: 0 | this.timeout,
                extraProperties,
            };
        }
    }

    function handleFormSubmission(options) {
        options || (options = {});
        const { formId } = options;

        if (!formId) {
            logError("No form to upload identified!");
            return false;
        }

        const form = _document.getElementById(formId);
        if (!form) {
            logError(`Form with id #${formId} not found!`);
            return false;
        }

        form.onsubmit = async () => {
            logDebug("Submitting form...");
            let cancel = false;
            const { beforeSubmit } = options;

            if (_isFunction(beforeSubmit)) cancel = await beforeSubmit.call(form);

            if (cancel) {
                logInfo("Form submissing was cancelled.");
                return false;
            }

            // With nowadays (modern) browsers, this is VERY unlikely, but just in case...
            if (!_supportsAJAXwithUpload) {
                logWarning("AJAX upload with progress report not supported.");

                cancel = await raiseAjaxUploadWithProgressNotSupported();

                if (cancel) {
                    // do not allow full page refresh
                    logInfo("Blocked submitting form with full-page refresh.");
                    return false;
                }

                // allow normal form submission (post back with full page refresh)
                return true;
            }

            const xhr = new XMLHttpRequest();
            const { beforeSend, done, fail, getFormData } = options;

            xhr.onreadystatechange = function () {
                if (this.readyState === XHRSTATE.DONE) {
                    // any status code between 200 and 299 inclusive is success
                    if (this.status > 199 && this.status < 300) {
                        if (_isFunction(done)) {
                            done.call(this);
                        }
                    } else if (_isFunction(fail)) {
                        fail.call(this);
                    }
                }
            };

            let { formData = null, hasFiles = false } = await getFormData();

            if (!formData) {
                formData = new FormData(form);
                const files = form.querySelectorAll("input[type=file]");

                // check if any of the inputs contains at least one file to upload
                for (let i = 0; i < files.length; i++) {
                    const input = files[i];
                    if (!!input.files && input.files.length) {
                        hasFiles = true;
                        break;
                    }
                }
            }

            setupXhrUploadHandlers(xhr, hasFiles);

            const url = form.getAttribute("action");
            const method = (form.getAttribute("method") || "POST").toUpperCase();

            logDebug("Form action URL is:", url);
            logDebug("Form method is:", method);

            xhr.open(method, url, true);

            let headersSet = false;

            if (_isFunction(beforeSend)) {
                const xhrResult = await beforeSend.call(xhr);
                logDebug("beforeSend was called:", xhrResult);

                if (_isObject(xhrResult)) {
                    if (xhrResult.cancel) {
                        logDebug("XMLHttpRequest.send was cancelled.");
                        return false;
                    }

                    logDebug("Trying to set XMLHttpRequest (XHR) properties...");
                    try {
                        headersSet = setRequestHeaders(xhr, xhrResult.requestHeaders);
                        xhr.withCredentials = xhrResult.withCredentials;
                    } catch (e) {
                        logError("Error setting XHR properties.", e);
                        if (_isFunction(fail)) fail.call(xhr, e.message);
                        return false;
                    }
                }
            }

            if (!headersSet)
                setRequestHeaders(xhr, _managerOptions.requestHeaders);

            xhr.send(formData);
            return false; // To avoid actual submission of the form
        }

        _form = form;
        return true;
    }

    function showOptionsUsage(isError) {
        const optionalNameOfMethodWhen = "string (optional): The name of the .NET method to invoke when ";
        const options = {
            formId: "string (required): The unique identifier of the form to submit.",

            assembly: "string (optional): The name of the .NET assembly on which " +
                "to invoke static methods. Required if any of the 'onXXX' static " +
                "method names are set.",

            logLevel: "number (optional): The level of details when logging messages " +
                "to the console. Supported values are: 0 (none), 1 (information), " +
                "2 (warning), 3 (error), 4 (debug). The default is 3.",

            requestHeaders: "object (optional): A dictionary of key-value pairs to set on an instance of XMLHttpRequest.",

            onGetModel: optionalNameOfMethodWhen + "retrieving the form model.",
            onBeforeSubmit: optionalNameOfMethodWhen + "submitting the form. This event can be cancelled.",
            onBeforeSend: optionalNameOfMethodWhen + "sending the HTTP request. This event can be cancelled.",
            onSendFailed: optionalNameOfMethodWhen + "the HTTP request fails.",
            onSendSucceeded: optionalNameOfMethodWhen + "the HTTP request completes successfully.",
            onUploadChanged: optionalNameOfMethodWhen + "an upload operation occurs.",
            onAjaxUploadWithProgressNotSupported: optionalNameOfMethodWhen + "the browser does not support AJAX uploads with progress report.",
        };

        const message = "Initialization options must be a variation of the following format:";

        if (isError) logError(message, options);
        else logInfo(message, options);

        return false;
    }

    function validateFormAttributes(formId) {
        const frm = _document.getElementById(formId);
        if (!frm) {
            logError(`The form element identified by '${formId}' does not exist in the DOM.`);
            return false;
        }
        if (!_isString(frm.getAttribute("action"))) {
            logError("The form action attribute is not defined.");
            return false;
        }
        return true;
    }

    /**
     * Recursively append to the specified form data 
     * properties and values from the given model.
     * @param {any} model The model from which to collect form data.
     * @param {FormData} formData The FormData object to which collected values are appended.
     * @param {string} path The current navigation path to a given property of the model.
     */
    function collectModelData(model, formData, path) {
        if (_isObject(model) && !(model instanceof Date) && !(model instanceof File) && !(model instanceof Blob)) {
            Object.keys(model).forEach(key => {
                collectModelData(model[key], formData, path ? `${path}[${key}]` : key);
            });
        } else {
            const value = (model === null || model === undefined) ? '' : model;
            formData.append(path, value);
        }
    }

    function containsFiles(form) {
        const inputs = form.querySelectorAll("input[type=file]");
        for (let i = 0; i < inputs.length; i++) {
            if (inputs[i].files && inputs[i].files.length) {
                return true;
            }
        }
        return false;
    }

    async function raiseAjaxUploadWithProgressNotSupported() {
        let cancel = false;
        if (!!_managerOptions) {
            const { onAjaxUploadWithProgressNotSupported } = _managerOptions;

            if (_isString(onAjaxUploadWithProgressNotSupported)) {
                const navigatorInfo = {};
                populateDictionary(navigatorInfo, global.navigator);
                logDebug("Navigator properties collected", navigatorInfo);

                cancel = await invokeDotNet(onAjaxUploadWithProgressNotSupported,
                    { extraProperties: navigatorInfo }
                );
            }
        }
        return cancel;
    }

    function supportsAjaxUploadWithProgress() {
        return supportsFileAPI() &&
            supportsAjaxUploadProgressEvents() &&
            supportsFormData();

        function supportsFileAPI() {
            var fi = _document.createElement("INPUT");
            fi.type = "file";
            return "files" in fi;
        };

        function supportsAjaxUploadProgressEvents() {
            var xhr = "XMLHttpRequest" in global && new XMLHttpRequest();
            return !!(xhr && ("upload" in xhr) && ("onprogress" in xhr.upload));
        };

        function supportsFormData() {
            return !!global.FormData;
        }
    }

    function invokeDotNet(method, arg) {
        if (arg === undefined) arg = null;

        if (!!_invokableInstanceRef) {
            logDebug(`Invoking .NET method "${method}" using object instance.`, arg);
            return _invokableInstanceRef.invokeMethodAsync(method, arg);
        } else {
            logDebug(`Invoking .NET method "${method}" using static type`, arg);
            return _dotnet.invokeMethodAsync(_managerOptions.assembly, method, arg);
        }
    }

    function setupXhrUploadHandlers(xhr, hasFiles) {
        const { onUploadChanged } = _managerOptions;
        if (!_isString(onUploadChanged)) return;

        async function handleEvent(e) {
            return invokeDotNet(onUploadChanged, {
                bytesSent: e.loaded,
                totalBytesToSend: e.total,
                eventType: UPLOAD_EVENTS[e.type],
                hasFiles
            });
        }

        logDebug("Setting up upload event handlers...");

        const { upload } = xhr;

        // The upload has begun.
        upload.addEventListener('loadstart', handleEvent);

        // The upload completed successfully.
        upload.addEventListener('load', handleEvent);

        /**
         * The upload finished. This event does not differentiate between
         * success or failure, and is sent at the end of the upload
         * regardless of the outcome. Prior to this event, one of load,
         * error, abort, or timeout will already have been delivered to
         * indicate why the upload ended.
         */
        upload.addEventListener('loadend', handleEvent);

        // Periodically delivered to indicate the amount of progress made so far.
        upload.addEventListener('progress', function (e) {
            handleEvent(e).then(cancel => (cancel) && xhr.abort());
        });

        // The upload failed due to an error.
        upload.addEventListener('error', handleEvent);

        // The upload operation was aborted.
        upload.addEventListener('abort', handleEvent);

        // The upload timed out because a reply did not arrive within 
        // the time interval specified by the XMLHttpRequest.timeout.
        upload.addEventListener('timeout', handleEvent);
    }

    function setRequestHeaders(xhr, headers) {
        if (!_isDictionary(headers)) return false;

        logDebug("Setting request headers...");

        for (const name in headers) {
            if (headers.hasOwnProperty(name)) {
                const value = headers[name];
                logDebug("Header:", { name, value });
                xhr.setRequestHeader(name, value);
            }
        }

        return true;
    }

    function logInfo() {
        log(CONSOLE_FUNC.info, ...arguments);
    }

    function logWarning() {
        log(CONSOLE_FUNC.warn, ...arguments);
    }

    function logError() {
        log(CONSOLE_FUNC.error, ...arguments);
    }

    function logDebug() {
        log(CONSOLE_FUNC.log, ...arguments);
    }

    function log(func, ...args) {
        const { logLevel = LOG_LEVEL.debug } = _managerOptions;

        if (logLevel === LOG_LEVEL.none) return;

        func || (func = CONSOLE_FUNC.info);
        const argument = [`${func}: BlazorFormManager:`, ...args];

        if (logLevel === LOG_LEVEL.debug)
            console[func].apply(console, argument);

        else if (func === CONSOLE_FUNC.info && logLevel === LOG_LEVEL.information) {
            console.info.apply(console, argument);

        } else if (func === CONSOLE_FUNC.warn && logLevel === LOG_LEVEL.warning) {
            console.warn.apply(console, argument);

        } else if (func === CONSOLE_FUNC.error && logLevel === LOG_LEVEL.error) {
            console.error.apply(console, argument);
        }
    }

    function _isString(obj) {
        return !!obj && typeof obj === "string" && ("" + obj).trim().length;
    }

    function _isObject(obj) {
        return !!obj && typeof obj === "object";
    }

    function _isFunction(obj) {
        return !!obj && typeof obj === "function";
    }

    function _isDictionary(obj) {
        return _isObject(obj) && Object.keys(obj).length;
    }

    /**
     * Builds a dictionary of primitive types.
     * @param {any} dic
     * @param {any} obj
     */
    function populateDictionary(dic, obj) {
        if (!obj) return;
        for (const prop in obj) {
            const value = obj[prop];
            const type = typeof value;
            if (value === undefined || value === null) continue;
            if (value instanceof Array || PRIMITIVES.indexOf(type) > -1) {
                dic[prop] = value;
            }
        }
    }

    async function fetchRequest(url, method, headers, credentials) {
        const result = await fetch({
            url,
            method,
            headers,
            credentials,
            cache: "no-cache"
        });
        return result;
    }
})(window);
