import { logDebug } from "./ConsoleLogger";
import { insertScripts, insertStyles, _isString } from "./Utils";

const global: any = globalThis;

const throwDependenciesMissing = () => {
    throw new Error("Couldn't install Quill dependencies.");
};

const parseDelta = delta => _isString(delta) ? JSON.parse(delta) : delta;

let Quill = global.Quill;
let _installed: boolean;
let _installing: boolean;
let _pendingResolvers: Array<Function> = [];

/**
 * Provides easy Quill (https://quilljs.com) integration, the 
 * free, open source WYSIWYG editor built for the modern web.
 */
export class QuillEditor implements IQuillEditor {
    private instance: any = null;

    /**
     * Initialize a new instance of the QuillEditor class.
     * @param selector The selector that targets the element used as the editor.
     * @param options Optional: The options to use.
     */
    constructor(private selector: string | HTMLElement, private options: QuillOptions, private installOptions?: QuillInstallOptions) {
        const { noinit, onready } = this.options = Object.assign({ theme: 'snow' }, options);
        
        if (!this.options.modules) {
            logDebug(this.options.formId, 'Setting default Quill modules!');
            this.options.modules = {
                'toolbar': [[{ 'font': [] }, { 'size': [] }], ['bold', 'italic', 'underline', 'strike'], [{ 'color': [] }, { 'background': [] }], [{ 'script': 'super' }, { 'script': 'sub' }], [{ 'header': [false, 1, 2, 3, 4, 5, 6] }, 'blockquote', 'code-block'], [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }], ['direction', { 'align': [] }], ['link', 'image', 'video'], ['clean']]
            }
        }

        // simple security token for invoking a method on the dotnet object reference
        const changeToken = this.options['changeToken'];

        // don't leave it in an easily accessible memory location
        delete this.options['changeToken'];

        if (!this.installOptions)
            this.installOptions = Object.assign({}, this.options);

        if (!noinit)
            this.init().then((success: boolean) => {
                if (success) {
                    this.setupChangeHandler(changeToken);
                    if (onready) onready(this);
                }
            });
    }

    /** Get the Quill editor instance for the current selector. */
    get quill() {
        if (!this.instance && !Quill)
            throw new Error("The editor has not been initialized yet! Call await init()");
        return this.instance || (this.instance = new Quill(this.selector, this.options));
    }

    /** Install Quill dependencies (if required), and initialize the Quill instance. */
    async init() {
        if (this.instance) return this.instance;
        if (!Quill) {
            const success = await QuillEditor.install(this.installOptions);
            if (!success) throwDependenciesMissing();
        }
        return this.quill;
    }

    /**
     * Get the contents of the editor at the specified index.
     * @param index The zero-based index at which to get the index.
     * @param length The number of characters to return.
     */
    getContents(index = 0, length?: number) {
        return this.quill.getContents(index, length);
    }

    /**
     * Set the contents of the editor.
     * @param delta The delta object or string to set.
     */
    setContents(delta) {
        return this.quill.setContents(parseDelta(delta));
    }

    /**
     * Update the contents of the editor.
     * @param delta The delta object or string to set.
     */
    updateContents(delta) {
        return this.quill.updateContents(parseDelta(delta));
    }

    /**
     * Delete the text at the specified index.
     * @param index The zero-based index at which to delete the text.
     * @param length The number of characters to delete.
     */
    deleteText(index: number, length: number) {
        return this.quill.deleteText(index, length);
    }

    /**
     * Return the text at the specified index.
     * @param index The zero-based index at which to get the text.
     * @param remaining The number of the remaining characters to return.
     */
    getText(index = 0, remaining?: number) {
        return this.quill.getText(index, remaining) as string;
    }

    /** Get the inner HTML of the root in the editor. */
    getInnerHTML() {
        return this.quill.root.innerHTML as string;
    }

    /**
     * Insert an embedded object at the specified index.
     * @param index The zero-based index at which to insert the object.
     * @param type The type of object to insert.
     * @param value The value of the object to insert.
     */
    insertEmbed(index: number, type: string, value: any) {
        return this.quill.insertEmbed(index, type, value);
    }

    /**
     * Insert some text at the specified index.
     * @param index The zero-based index at which to insert the text.
     * @param text The text to insert.
     * @param format The format of the text to insert.
     * @param value The value of the text to insert.
     * @param formats A dictionary specifying the formats of the text to insert.
     */
    insertText(index: number, text: string, format?: string, value?: any, formats?: { [key: string]: any }) {
        if (format)
            return this.quill.insertText(index, text, format, value);
        return this.quill.insertText(index, text, value, formats);
    }

    /**
     * Format the contents
     * @param name The name of the format.
     * @param value The value of the format.
     */
    format(name: string, value: any) {
        return this.quill.format(name, value);
    }

    /**
     * Enable or disable the editor.
     * @param enabled true if the editor is enabled; otherwise, false.
     */
    enable(enabled: boolean) {
        this.quill.enable(enabled);
    }

    /** This function is called when the editor's dependencies have been successfully loaded. */
    private setupChangeHandler(changeToken: string) {
        const { selector } = this;
        // the dotnet object reference used to report back edit changes
        const dotnetObj = this.options['dotNetObjectReference'];

        if (dotnetObj || (typeof selector === 'string' && selector.startsWith('#'))) {
            // the selector is an element identifier;
            // the C# class AutoInputBase adds a hidden input for
            // every rich text-enabled textarea; we'll use that
            // input to store the changes made through the editor; this
            // way, the value will be included during the form submission

            let hiddenInput: HTMLInputElement;

            if (typeof selector === 'string') {
                const inputId = selector.substring(1) + '_hidden';
                hiddenInput = document.getElementById(inputId) as HTMLInputElement;
            }

            if (hiddenInput || dotnetObj) {
                // the dotnet method to invoke upon change detection
                const onValueChanged = this.options['onValueChanged'] ?? 'OnValueChanged';

                this.quill.on('text-change', async () => {
                    const value = this.getInnerHTML();

                    if (hiddenInput)
                        hiddenInput.value = value;

                    if (dotnetObj)
                        // invoke the dotnet method with the changed value
                        await dotnetObj.invokeMethodAsync(onValueChanged, value, changeToken);
                });
            }
        }
    }

    /**
     * Install Quill Editor's dependencies.
     * @param options The base installation options to use.
     */
    static install(options?: QuillInstallOptions) {
        return new Promise<boolean>((resolve) => {
            if (_installed) {
                resolve(true);
            } else if (_installing) {
                // installation in progress; queue subsequent resolvers
                _pendingResolvers.push(resolve);
            } else {
                _installing = true;
                // queue the first resolver
                _pendingResolvers.push(resolve);

                const { theme = 'snow', version = '1.3.6' } = options;
                const basepath = `https://cdn.quilljs.com/${version}`;
                const {
                    scriptUrl = `${basepath}/quill.min.js`,
                    styleUrl = `${basepath}/quill.${theme}.css`
                } = options;

                insertStyles(styleUrl);

                insertScripts(scriptUrl, function () {
                    // installation done
                    Quill = global.Quill;
                    _installed = !!Quill;
                    while (_pendingResolvers.length) {
                        // get, invoke, and then remove the resolver
                        const r = _pendingResolvers[0];
                        r.call(r, _installed);
                        _pendingResolvers.splice(0, 1);
                    }
                    _installing = false;
                });
            }
        });
    }

    /**
     * Register a Quill module.
     * @param name The name of the module to register.
     * @param classDef The implementation class of the module.
     */
    static async registerModule(name: string, classDef: any) {
        if (!Quill) {
            const success = await QuillEditor.install();
            if (success) register();
            else throwDependenciesMissing();
        } else register();

        function register() {
            Quill.register('modules/' + name, classDef, true);
        }
    }
}
