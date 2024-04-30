import { Forms } from "./Shared";
import { logDebug, logError, logWarning } from "./ConsoleLogger";
import { _isString, _isObject, _isFunction, _isDictionary } from "./Utils";
import { SimpleEvent } from "./SimpleEvent";

const global = globalThis;
const RECAPTCHA_SCRIPT_BASE_URL = 'https://www.google.com/recaptcha/api.js';
const OPTIONS: {[siteKey: string]: ReCaptchaOptions} = {};

/** Seamlessly integrates Google's reCAPTCHA technology. */
export class ReCAPTCHA {
    /**
     * Initialize a new instance of the ReCAPTCHA class.
     */
    constructor() {
    }

    private _greCAPTCHA: ReCaptchaConfig = {
        callbacks: {},
        scripts: {
            v1: '',
            v2: RECAPTCHA_SCRIPT_BASE_URL + '?onload=BlazorFormManagerReCaptchaOnload&render=explicit',
            v3: RECAPTCHA_SCRIPT_BASE_URL + '?render=',
            inserted: false,
            inserting: false
        }
    };

    private _activityEvent = new SimpleEvent<ReCaptchaActivity>();

    /** Expose the activity event. */
    get activity() {
        return this._activityEvent.expose();
    }

    /** Return the reCAPTCHA options. */
    get options() {
        return OPTIONS;
    }

    /**
     * Configure the reCAPTCHA for the specified form identifier.
     * @param formId The form identifier.
     * @param reCaptcha The configuration options.
     * @param isReconfiguring Indicates whether the configuration is being repeated.
     */
    configure(formId: string, reCaptcha: ReCaptchaOptions, isReconfiguring = false) {
        const { scripts } = this._greCAPTCHA;
        isReconfiguring || (isReconfiguring = !reCaptcha);

        reCaptcha
            ? (OPTIONS[reCaptcha.siteKey] = reCaptcha)
            : (reCaptcha = this.getFirstOption() || { siteKey: '', version: '' });

        if (!reCaptcha) {
            logError(formId, 'Cannot configure reCAPTCHA when the options are missing.');
            return false;
        }

        logDebug(formId, 'Configuring reCAPTCHA with options', reCaptcha);

        const {
            version,
            verificationTokenName,
            invisible,
        } = reCaptcha;

        const ver = (version || '').toLowerCase();
        let url = scripts[ver] + '';
        let success = false;

        if (!_isString(url)) {
            const message = `Unsupported reCAPTCHA version: ${ver}`;
            logError(formId, message);
            this.reportActivity(formId, { formId, message, type: 'danger' });
        } else {
            if (ver === 'v2') {
                // register version 2 callback for this form
                const { callbacks } = this._greCAPTCHA;
                const cbConfig: ReCaptchaCallback = {};

                // reCAPTCHA execution callback used to 
                // collect the token and submit the form
                cbConfig['setTokenCallback'] = (token) => {
                    logDebug(formId, 'reCAPTCHA token received; preparing to submit the form.', token);

                    if (invisible) {
                        // these properties must be set in the 'handleFormSubmission' 
                        // function when the form is being submitted
                        const { xhr, formData } = cbConfig;

                        if (xhr && formData) {
                            this.reportActivity(formId);
                            const obj = this.getData(formId, formData, verificationTokenName, token, ver);
                            xhr.send(obj);
                        } else {
                            const message = 'reCAPTCHA: xhr and formData properties not set.';
                            logError(formId, message);
                            this.reportActivity(formId, { formId, message, type: 'danger' });
                        }
                    } else {
                        // store the token; it will be used when sending the form
                        cbConfig.token = token;

                        // clear previous (danger|warning, etc.) message
                        this.reportActivity(formId);
                    }
                };

                callbacks[formId] = cbConfig;
                    
                if (!global.BlazorFormManagerReCaptchaOnload) {
                    // fired when reCAPTCHA is ready
                    global.BlazorFormManagerReCaptchaOnload = () => this.onload(formId, reCaptcha);
                } else {
                    this.onload(formId, reCaptcha);
                }
            }

            if (isReconfiguring) {
                scripts.inserted = !this.requiresReCaptcha(formId);
                scripts.inserting = false;
            }

            this.insertScript(formId, reCaptcha);
            success = true;
        }

        return success;

    }

    /**
     * Attempt to insert a Google reCAPTCHA script into the document.
     * @param formId The form identifier.
     * @param reCaptcha The reCAPTCHA configuration options.
     * @returns {boolean} true if reCAPTCHA is required; otherwise, false.
     */
    insertScript(formId?: string, reCaptcha?: ReCaptchaOptions) {
        logDebug(formId, 'Attempting reCAPTCHA script tag insertion...');

        const { scripts, } = this._greCAPTCHA;
        const shouldInsert = !scripts.inserted && !scripts.inserting && this.requiresReCaptcha(formId);

        if (shouldInsert) {
            scripts.inserting = true;

            reCaptcha
                ? (OPTIONS[reCaptcha.siteKey] = reCaptcha)
                : (reCaptcha = this.getFirstOption() || { siteKey: '', version: '' });

            const {
                siteKey,
                version,
            } = reCaptcha;

            const ver = (version || '').toLowerCase();
            let url = scripts[ver] + '';

            if (ver === 'v3') url += siteKey;
            logDebug(formId, 'Inserting reCAPTCHA script tag...', url);

            const s = document.createElement('script');
            const heads = document.getElementsByTagName('head');

            s.async = true;
            s.defer = true;
            s.src = url;

            s.onload = () => {
                scripts.inserted = true;
                scripts.inserting = false;
                this.reportActivity(formId, { formId, message: 'reCAPTCHA script loaded.', type: 'scriptload' });
            };

            s.onerror = () => {
                heads[0].removeChild(s);
                scripts.inserted = false;
                scripts.inserting = false;
                this.reportActivity(formId, { formId, message: 'Failed to load reCAPTCHA script.', type: 'scriptloaderror' });
            };

            if (heads.length === 0) {
                const h = document.createElement('head');
                document.appendChild(h);
                h.appendChild(s);
            } else {
                heads[0].appendChild(s);
            }

            logDebug(formId, 'reCAPTCHA script tag inserted successfully!');
            return true;
        } else if (scripts.inserted) {
            logDebug(formId, 'reCAPTCHA script has already been inserted.');
        } else if (scripts.inserting) {
            logDebug(formId, 'reCAPTCHA script is being inserted.');
        }
        return false;
    }

    /**
     * Check if the document requires reCAPTCHA.
     * @param formId The form identifier.
     * @returns {boolean} true if reCAPTCHA is required; otherwise, false.
     */
    requiresReCaptcha(formId?: string) {
        const requires = (
            // check if there's any form in the document
            document.querySelectorAll('form').length &&

            // make sure there's no script starting with the reCAPTCHA source
            document.querySelectorAll(`script[src^="${RECAPTCHA_SCRIPT_BASE_URL}"]`).length === 0
        );
        formId && logDebug(formId, 'Is reCAPTCHA required?', requires);
        return requires;
    }

    /**
     * Return the reCAPTCHA options for the specified site key.
     * @param siteKey The reCAPTCHA site key identifier.
     * @returns {ReCaptchaOptions | undefined}
     */
    getOptions(siteKey: string) : ReCaptchaOptions | undefined {
        return OPTIONS[siteKey];
    }

    /**
     * Return the first reCAPTCHA options, if any.
     * @returns
     */
    getFirstOption() {
        const keys = Object.keys(OPTIONS);
        for (let i = 0; i < keys.length; i++) {
            if (OPTIONS[keys[i]]) return OPTIONS[keys[i]];
        }
        return undefined;
    }

    /**
     * Reset previously created reCAPTCHA widgets.
     * @param formId The form identifier.
     * @param reCaptcha The configuration options.
     */
    reset(formId: string, reCaptcha: ReCaptchaOptions) {
        reCaptcha || (reCaptcha = this.getFirstOption());

        if (!reCaptcha) {
            logError(formId, 'reCAPTCHA argument to reset is not defined!');
            return;
        }

        const grecaptcha = globalThis['grecaptcha'];

        if (!grecaptcha) {
            logError(formId, 'Google reCAPTCHA not loaded!');
            return;
        }

        const { version } = reCaptcha;
        const ver = version.toLowerCase();

        if (ver === 'v3') {
            logDebug(formId, 'Google reCAPTCHA version 3 is executed when the form is submitted.');
            return;
        }

        // get the widget identifers that have been rendered
        const { widgets } = this.getCallback(formId);

        if (widgets && widgets.length) {

            for (let i = 0; i < widgets.length; i++)
                grecaptcha.reset(widgets[i]);

            const message = `reCAPTCHA was reset.`;
            logDebug(formId, message);

            this.reportActivity(formId, { formId, message, type: 'warning' });
        } else {
            logDebug(formId, 'No reCAPTCHA widgets found to reset!');
        }
    }

    /**
     * Report an activity related to Google's reCAPTCHA technology.
     * @param formId The form identifier.
     * @param activity The activity to report.
     */
    reportActivity(formId: string, activity?: ReCaptchaActivity) {
        const { onReCaptchaActivity } = Forms[formId] || {};
        if (_isString(onReCaptchaActivity)) {
            if (!activity)
                activity = { formId, message: '', type: '', data: null };
            if (activity.data === undefined)
                activity.data = null;
            this._activityEvent.trigger(activity);
        }
    }

    /**
     * Execute a reCAPTCHA challenge to obtain a token, and submit the identified form with it.
     * @param formId The form identifier.
     * @param xhr The object used to send the form.
     * @param data The form data to send.
     * @param reCaptcha The reCAPTCHA configuration options.
     */
    submitForm(formId: string, xhr: XMLHttpRequest, data: FormData | string, reCaptcha: ReCaptchaOptions) {
        const grecaptcha = global.grecaptcha;

        if (!grecaptcha) {
            logError(formId, "Google reCAPTCHA is not globally available.");
            xhr.send(data);
            return false;
        }

        const { siteKey, version, verificationTokenName, allowLocalHost, invisible, size } = reCaptcha || {};
        let success = false;

        if (_isString(siteKey)) {
            if (!allowLocalHost && global.location.hostname.toLowerCase().indexOf('localhost') > -1) {
                const message = "reCAPTCHA not allowed on localhost; sending form using XHR.";
                logWarning(formId, message);
                this.reportActivity(formId, { formId, message, type: 'warning' });
                xhr.send(data);
            } else {
                let ver = version.toLowerCase();
                logDebug(formId, "Trying to submit form using reCAPTCHA version " + ver);

                if (ver === 'v2') {
                    const cbConfig = this.getCallback(formId);

                    if (!cbConfig) {
                        const message = 'No callback for reCAPTCHA v2 has been ' +
                            'configured for this form; submitting without the challenge.';

                        logWarning(formId, message);
                        this.reportActivity(formId, { formId, message, type: 'warning' });
                        xhr.send(data);
                    } else {
                        if (invisible || size === 'invisible') {
                            // set these properties so that the reCAPTCHA 
                            // callback may use them to send the request
                            cbConfig.xhr = xhr;
                            cbConfig.formData = data;

                            // execution, if successful, will delegate the form submission
                            grecaptcha.execute();
                        } else {
                            const token = cbConfig.token || '';
                            if (!_isString(token)) {
                                const message = "Please make sure to complete the reCAPTCHA challenge!";
                                logError(formId, message);
                                this.reportActivity(formId, { formId, message, type: 'danger' });
                            } else {
                                const obj = this.getData(formId, data, verificationTokenName, token, ver);
                                xhr.send(obj);
                                success = true;
                            }
                        }
                    }
                } else if (ver === 'v3') {
                    grecaptcha.ready(() => {
                        grecaptcha.execute(siteKey, { action: 'submit' }).then((token: string) => {
                            const message = "reCAPTCHA token received.";
                            logDebug(formId, message);
                            this.reportActivity(formId, { formId, message, type: 'info', data: token });
                            const obj = this.getData(formId, data, verificationTokenName, token, ver);
                            xhr.send(obj);
                            success = true;
                        });
                    });
                } else {
                    const message = 'Unsupported reCAPTCHA version: ' + ver;
                    logWarning(formId, message);
                    this.reportActivity(formId, { formId, message, type: 'warning', data: ver });
                    xhr.send(data);
                }
            }
        } else {
            logWarning(formId, "reCAPTCHA form key not defined; sending form using XHR.");
            xhr.send(data);
        }

        return success;

    }

    private getData(formId: string, data: FormData | string, verificationTokenName: string, token: string, ver: string) {
        if (data instanceof FormData) {
            logDebug(formId, 'Setting form data values.')
            data.set(verificationTokenName, token);
            data.set('g-recaptcha-version', ver);
            return data;
        } else {
            const json = JSON.parse(data);
            logDebug(formId, 'Converted data to JSON prior to setting the verification token name and version.', json);
            json[verificationTokenName] = token;
            json['g-recaptcha-version'] = ver;
            return JSON.stringify(json);
        }
    }

    private onload(formId: string, reCaptcha: ReCaptchaOptions) {
        const {
            siteKey,
            theme: them,
            languageCode,
            invisible,
            size: sz,
            cssSelector
        } = reCaptcha;

        const selector = cssSelector || '.g-recaptcha';
        const form = document.getElementById(formId);
        const elements = form && form.querySelectorAll(selector) || [];
        const count = elements.length;

        if (count > 0) {
            logDebug(formId, `${count} HTML element(s) with ${selector} CSS selector found.`);

            let hl = '', size = sz, theme = them;

            if (languageCode)
                hl = languageCode;

            if (invisible)
                reCaptcha['size'] = size = 'invisible';
            else if (!_isString(size))
                reCaptcha['size'] = size = 'normal';

            if (!_isString(theme))
                theme = 'light';

            const cbConfig = this.getCallback(formId);
            
            const parameters = {
                hl,
                size,
                theme,
                'sitekey': siteKey,
                'callback': cbConfig.setTokenCallback
            };

            // since a reCAPTCHA response is verified only once,
            // we have to store the rendered widget IDs so that 
            // we can reset them later, should the submission fail
            const widgets: number[] = [];
            const gr = global.grecaptcha;

            for (let i = 0; i < count; i++)
                // grecaptcha.render returns an ID for each created widget
                widgets.push(gr.render(elements[i], parameters));
            
            cbConfig['widgets'] = widgets;
        } else {
            logError(formId, `No HTML element with ${selector} CSS selector found.`);
        }
    }

    private getCallback(formId: string) {
        return this._greCAPTCHA.callbacks[formId] || {};
    }
}
