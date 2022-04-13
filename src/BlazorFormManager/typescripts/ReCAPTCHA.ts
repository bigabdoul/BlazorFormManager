import { Forms } from "./Shared";
import { logDebug, logError, logWarning } from "./ConsoleLogger";
import { _isString, _isObject, _isFunction, _isDictionary } from "./Utils";

const global = globalThis;
const RECAPTCHA_SCRIPT_BASE_URL = 'https://www.google.com/recaptcha/api.js';

/** Seamlessly integrates Google's reCAPTCHA technology. */
export class ReCAPTCHA {
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

    get activity() {
        return this._activityEvent.expose();
    }

    /**
     * Configure the reCAPTCHA for the specified form identifier.
     * @param formId The form identifier.
     * @param reCaptcha The configuration options.
     */
    configure(formId: string, reCaptcha: ReCaptchaOptions) {
        const { scripts } = this._greCAPTCHA;

        const {
            siteKey,
            version,
            verificationTokenName,
            invisible
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
                            formData.set(verificationTokenName, token);
                            formData.set('g-recaptcha-version', ver);
                            xhr.send(formData);
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

            if (ver === 'v3') url += siteKey;

            if (!(scripts.inserted || scripts.inserting)) {
                scripts.inserting = true;
                logDebug(formId, 'Inserting reCAPTCHA script tag...', url);

                const s = document.createElement('script');
                const heads = document.getElementsByTagName('head');

                s.async = true;
                s.defer = true;
                s.src = url;

                s.onload = () => {
                    scripts.inserted = true;
                    scripts.inserting = false;
                };

                s.onerror = () => {
                    heads[0].removeChild(s);
                    scripts.inserted = false;
                    scripts.inserting = false;
                };

                if (heads.length === 0) {
                    const h = document.createElement('head');
                    document.appendChild(h);
                    h.appendChild(s);
                } else {
                    heads[0].appendChild(s);
                }

                logDebug(formId, 'reCAPTCHA script tag inserted successfully!');
            }

            success = true;
        }

        return success;
    }

    /**
     * Reset previously created reCAPTCHA widgets.
     * @param formId The form identifier.
     * @param reCaptcha The configuration options.
     */
    reset(formId: string, reCaptcha: ReCaptchaOptions) {
        if (!reCaptcha) return;

        const grecaptcha = globalThis['grecaptcha'];

        if (!grecaptcha) return;

        const { version } = reCaptcha;
        const ver = version.toLowerCase();

        // version 3 is executed when the form is submitted
        if (ver === 'v3') return;

        // get the widget identifers that have been rendered
        const { widgets } = this.getCallback(formId);

        if (widgets && widgets.length) {

            for (let i = 0; i < widgets.length; i++)
                grecaptcha.reset(widgets[i]);

            const message = `reCAPTCHA was reset.`;
            logDebug(formId, message);

            this.reportActivity(formId, { formId, message, type: 'warning' });
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
     * @param formData The form data to send.
     * @param reCaptcha The reCAPTCHA configuration options.
     */
    submitForm(formId: string, xhr: XMLHttpRequest, formData: FormData, reCaptcha: ReCaptchaOptions) {
        const grecaptcha = global.grecaptcha;
        if (!grecaptcha) return false;

        const { siteKey, version, verificationTokenName, allowLocalHost, invisible, size } = reCaptcha || {};
        let success = false;

        if (_isString(siteKey)) {
            if (!allowLocalHost && global.location.hostname.toLowerCase().indexOf('localhost') > -1) {
                const message = "reCAPTCHA not allowed on localhost; sending form with XHR.";
                logWarning(formId, message);
                this.reportActivity(formId, { formId, message, type: 'warning' });
                xhr.send(formData);
                success = true;
            } else {
                let ver = version.toLowerCase();
                logDebug(formId, "Trying to submit form with reCAPTCHA version " + ver);

                if (ver === 'v2') {
                    const cbConfig = this.getCallback(formId);

                    if (!cbConfig) {
                        const message = 'No callback for reCAPTCHA v2 has been ' +
                            'configured for this form; submitting without the challenge.';

                        logWarning(formId, message);
                        this.reportActivity(formId, { formId, message, type: 'warning' });
                        xhr.send(formData);
                        success = true;
                    } else {
                        if (invisible || size === 'invisible') {
                            // set these properties so that the reCAPTCHA 
                            // callback may use them to send the request
                            cbConfig.xhr = xhr;
                            cbConfig.formData = formData;

                            // execution, if successful, will delegate the form submission
                            grecaptcha.execute();
                        } else {
                            const token = cbConfig.token || '';
                            if (!_isString(token)) {
                                const message = "Please make sure to complete the reCAPTCHA challenge!";
                                logError(formId, message);
                                this.reportActivity(formId, { formId, message, type: 'danger' });
                            } else {
                                formData.set(verificationTokenName, token);
                                formData.set('g-recaptcha-version', ver);
                                xhr.send(formData);
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
                            formData.set(verificationTokenName, token);
                            formData.set('g-recaptcha-version', ver);
                            xhr.send(formData);
                            success = true;
                        });
                    });
                } else {
                    const message = 'Unsupported reCAPTCHA version: ' + ver;
                    logWarning(formId, message);
                    this.reportActivity(formId, { formId, message, type: 'warning', data: ver });
                    xhr.send(formData);
                    success = true;
                }
            }
        }

        return success;
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
