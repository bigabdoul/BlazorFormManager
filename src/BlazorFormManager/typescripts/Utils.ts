import { Forms, PRIMITIVES, SIZE_KB, SIZE_MB, SIZE_GB, SIZE_TB, DefaultImagePreviewOptions, DragDropStorage } from "./Shared";
import { logDebug } from "./ConsoleLogger";

/** A general-purpose utility class. */
class Utils {

    /**
     * Check if the specified object is defined and is a non-empty string.
     * @param obj
     */
    static _isString(obj) {
        return obj && typeof obj === "string" && ("" + obj).trim().length > 0;
    }

    /**
     * Check if the specified object is defined and its type is 'object'.
     * @param obj
     */
    static _isObject(obj) {
        return obj && typeof obj === "object";
    }

    /**
     * Check if the specified object is defined and its type is 'function'.
     * @param obj
     */
    static _isFunction(obj) {
        return obj && typeof obj === "function";
    }

    /**
     * Check if the specified object is a defined object with one or more keys.
     * @param obj
     */
    static _isDictionary(obj) {
        return Utils._isObject(obj) && Object.keys(obj).length > 0;
    }

    /**
     * Builds a dictionary of primitive types.
     * @param {any} target
     * @param {any} source
     */
    static populateDictionary(target, source) {
        for (const prop in source) {
            const value = source[prop];
            const type = typeof value;
            if (value === undefined || value === null) continue;
            if (PRIMITIVES.indexOf(type) > -1) {
                target[prop] = value;
            }
            //if (value instanceof Array) {
            //    for (let i = 0; i < value.length; i++) {

            //    }
            //}
        }
        return target;
    }

    /**
     * Check if the specified form contains at least one file to upload.
     * @param form The form to query.
     */
    static containsFiles(form: HTMLFormElement) {
        const inputs = form.querySelectorAll("input[type=file]");
        for (let i = 0; i < inputs.length; i++) {
            const input = inputs[i] as HTMLInputElement;
            if (input.files && input.files.length) {
                return true;
            }
        }
        return false;
    }

    /**
     * Create a file info object from the specified file.
     * @param {File} file
     * @param {{width: number, height: number}} dimensions Optional: The dimensions of the file if it's an image.
     * @param {boolean} createUrl true to create an object URL if the file type is an image.
     */
    static createFileInfo(file: File, dimensions?: { width: number, height: number }, createUrl?: boolean) {
        if (!_isObject(dimensions)) dimensions = null;
        return {
            name: file.name,
            type: file.type,
            size: file.size,
            lastModifiedDate: new Date(file.lastModified),
            dimensions,
            objectUrl: createUrl && file.type.toLowerCase().indexOf('image/') > -1 ? URL.createObjectURL(file) : ""
        }
    }

    static supportsFileExtension(name: string, supportedFiles: string[]) {
        name = (name || "").toLowerCase();
        for (let i = 0; i < supportedFiles.length; i++)
            if (name.endsWith(supportedFiles[i]))
                return true;
        return false;
    }

    /**
     * Log an error message for the element that has not been found in the DOM tree.
     * @param {string} targetId The identifier of the not-found element.
     * @param {string} name Optional: The name of the element.
     */
    static notFound(targetId?: string, name?: string) {
        const error = `The ${name} element identified by #${targetId} has not been found in the DOM tree.`;
        return error;
    }

    /**
     * Convert size to a human-readable format.
     * @param {number} size The file size in bytes.
     */
    static fileSizeToString(size: number) {
        if (size <= 0) return "";
        if (size < SIZE_KB) return `${size} B`;
        if (size < SIZE_MB) return `${(size / SIZE_KB).toFixed(1)} KB`;
        if (size < SIZE_GB) return `${(size / SIZE_MB).toFixed(1)} MB`;
        return `${(size / SIZE_TB).toFixed(2)} GB`;
    }

    /**
     * Add classes to the element's classList collection.
     * @param {HTMLElement} element
     * @param {string} classes A whitespace-separated string of HTML classes.
     */
    static addClassList(element: HTMLElement, classes: string) {
        element.classList.add(...("" + classes).split(" ").map(cls => cls.trim()));
        return element;
    }
    
    static async getTargetElementById(formId: string, id: string, interop?: FormManagerInteropRef) {
        const ERROR_CODE_BASE = 300;
        if (_isString(id)) {
            const targetElement = document.getElementById(id);

            if (!targetElement) {
                const error = notFound(id);
                const result = { succeeded: false, error, targetElement, code: ERROR_CODE_BASE + 2 };

                if (interop && _isString(Forms[formId].onFileReaderResult))
                    await interop.invokeDotNet(formId, Forms[formId].onFileReaderResult, result);
                return result;
            }

            return { succeeded: true, error: undefined, targetElement, code: 0 };
        }

        return { succeeded: false, error: undefined, targetElement: undefined, code: ERROR_CODE_BASE + 1 }
    }

    
    static supportsImageUtil() {
        // before checking for the ImageUtil class, which uses the 
        // <canvas> element, make sure first that the device supports canvas
        if (!!window.CanvasRenderingContext2D &&
            !!globalThis.ImageUtility && globalThis.ImageUtility.prototype && globalThis.ImageUtility.prototype.constructor) {
            const util = new globalThis.ImageUtility("");
            return ("crop" in util) && ("resize" in util);
        }
        return false;
    }

    /**
     * Remove dropped files and auto-generated images for the specified target element.
     * @param {DragDropTargetOptions} options The options.
     */
    static removeFileList(options: DragDropTargetOptions) {
        const { formId, targetId } = options;
        logDebug(formId, `Attempting to remove dropped files for target element #${targetId}...`);

        if (targetId in DragDropStorage) {
            const config = DragDropStorage[targetId];
            const { options } = config;
            const { inputName, imagePreviewOptions = DefaultImagePreviewOptions } = options;

            if ('droppedFiles' in config) {
                delete config.droppedFiles[inputName];
                delete config.droppedFiles;
                logDebug(formId, "Files deleted from drag and drop storage.");
            } else {
                logDebug(formId, "No files have been dropped onto the dedicated area.");
            }

            if (!!imagePreviewOptions) {
                const { tagId: previewId, attributeName: attr } = imagePreviewOptions;
                if (_isString(previewId)) {
                    // suppress image preview, if any
                    const image = document.getElementById(previewId);
                    if (!!image) {
                        image.removeAttribute(attr || "src");
                        logDebug(formId, "Removed image preview.");
                    }
                    else
                        logDebug(formId, `Image #${previewId} not found in the DOM tree.`);
                }
            }

            const dropTarget = document.getElementById(targetId);

            if (dropTarget) dropTarget.classList.add("drag-drop-area-empty");

            Utils.removeAutoGeneratedImages(dropTarget);

            return true;
        } else {
            logDebug(formId, "Target element not found in drag and drop storage.");
        }
        return false;
    }

    /**
     * Enumerate all keys in the given form data object.
     * @param formData The form data to enumerate.
     */
    static formDataKeys(formData: FormData) {
        const keys: Array<string> = [];
        if ('keys' in formData && typeof formData['keys'] === 'function') {
            const fd: any = formData;
            for (const key of fd.keys()) {
                keys.push(key as string);
            }
        } else {
            formData.forEach((_, key, __) => {
                keys.push(key);
            });
        }
        return keys;
    }

    /**
     * Merge form data from source into target.
     * @param target The object that will receive the form data from source.
     * @param source The object whose data will be merged into target.
     */
    static formDataMerge(target: FormData, source: FormData) {
        source.forEach((value, key) => target.set(key, value));
        return target;
    }

    /**
     * Uniquely insert one or more stylesheets into the DOM.
     * @param sources A URL or array of URL of the stylesheets to load.
     * @param onload The event handler to fire when the browser loads a stylesheet.
     * @param formId The form identifier.
     */
    static insertStyles(sources: string | Array<string>, onload?: (this: GlobalEventHandlers, ev: Event) => any, formId?: string) {
        if (!(sources instanceof Array))
            sources = [sources];

        const head = Utils.getOrCreateHead();

        for (let i = 0; i < sources.length; i++) {
            const src = sources[i];
            const link = document.querySelector(`link[href="${src}"]`);
            const exists = !!link;

            if (exists) {
                const rel = link.attributes["rel"];
                if (!rel || rel.value === "stylesheet") {
                    logDebug(formId, `The CSS style ${src} is already in the DOM.`)
                    continue;
                }
            }

            let s = document.createElement("link");
            if (onload) s.onload = onload;
            s.href = src;
            s.rel = "stylesheet";
            head.appendChild(s);
        }
    }

    /**
     * Uniquely insert one or more scripts into the DOM.
     * @param sources A URL or array of URL of the scripts to load.
     * @param onload The event handler to fire when the browser loads a script.
     * @param formId The form identifier.
     */
    static insertScripts(sources: string | Array<string>, onload?: (this: GlobalEventHandlers, ev: Event) => any, formId?: string) {
        if (!(sources instanceof Array))
            sources = [sources];

        const head = Utils.getOrCreateHead();

        for (let i = 0; i < sources.length; i++) {
            const src = sources[i];
            const exists = !!document.querySelector(`script[src="${src}"]`);

            if (exists) {
                logDebug(formId, `The script ${src} is already in the DOM.`)
                continue;
            }

            const s = document.createElement("script");
            if (onload) s.onload = onload;
            s.src = src;
            s.async = true;
            s.defer = true;
            head.appendChild(s);
        }
    }

    /** Get, or create and insert, the 'head' DOM element. */
    static getOrCreateHead() {
        return Utils.getElementByTag("head", function (name) {
            const d = document;
            const h = d.createElement(name);
            return d.insertBefore(h, d.body);
        });
    }

    /**
     * Get the first element identified by name, or create a new one using factory.
     * @param name The tag name of the element to retrieve.
     * @param factory A fallback factory function or object.
     */
    static getElementByTag<K extends keyof HTMLElementTagNameMap>(name: K, factory?: ((name: K) => HTMLElementTagNameMap[K]) | any): HTMLElementTagNameMap[K] {
        const elms = document.getElementsByTagName(name);
        return elms && elms.length && elms[0] || (typeof factory === 'function' ? factory(name) : factory);
    }

    /**
     * Remove auto-generated images.
     * @param {HTMLElement} container Optional: The HTML element that contains the images. Defaults to document.
     */
    private static removeAutoGeneratedImages(container: HTMLElement) {
        const images = (container || document).querySelectorAll(`.${DefaultImagePreviewOptions.autoGeneratedClass}`);
        for (let i = 0; i < images.length; i++) {
            const img = images[i];
            img.remove();
        }
    }

}

const { _isDictionary, _isFunction, _isObject, _isString, populateDictionary, containsFiles, createFileInfo, supportsFileExtension, notFound, fileSizeToString, addClassList, getTargetElementById, supportsImageUtil, removeFileList, formDataKeys, formDataMerge, insertStyles, insertScripts, getElementByTag } = Utils;

export {
    _isDictionary,
    _isFunction,
    _isObject,
    _isString,
    populateDictionary,
    containsFiles,
    createFileInfo,
    supportsFileExtension,
    notFound,
    fileSizeToString,
    addClassList,
    getTargetElementById,
    supportsImageUtil,
    removeFileList,
    formDataKeys,
    formDataMerge,
    insertStyles,
    insertScripts,
    getElementByTag,
    Utils
}
