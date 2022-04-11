﻿import { logDebug, logError, logWarning } from "./ConsoleLogger";
import { ImageUtility } from "./ImageUtility";
import { Forms, DefaultImagePreviewOptions } from "./Shared";
import { addClassList, fileSizeToString, getTargetElementById, notFound, supportsImageUtil, _isObject, _isString } from "./Utils";

const _imageUtilAvailable = supportsImageUtil();

class ImagePreviewGenerator {


    /**
     * Takes care of generating an image preview for the specified file.
     * @param {string} dataURL URL-encoded image data.
     * @param {File} file The file whose dataURL is provided.
     * @param {object} options An object used to generate an image.
     */
    async generateImagePreview(dataURL: string, file: File, options: FileReaderOptions) {
        const { formId, inputId, imagePreviewOptions, multiple } = options;

        if (!_isObject(imagePreviewOptions)) {
            logWarning(formId, "No image preview generation options set.");
        } else {
            const previewOptions = this.defaultPreviewOptionsFrom(imagePreviewOptions);
            const {
                autoGenerate, tagId, tagName, wrapperSelector,
                width: desiredWidth, height: desiredHeight,
                preserveAspectRatio, noResize
            } = previewOptions;

            if (_isString(tagId)) {
                const result = await getTargetElementById(formId, tagId);
                const { succeeded, targetElement, error } = result;
                const image = targetElement as HTMLImageElement;

                if (succeeded) {
                    this.addImageAttributes(dataURL, file, image, previewOptions, multiple);
                    return {
                        width: (image.width || 0) | 0,
                        height: (image.height || 0) | 0
                    };
                }

                logError(formId, error);

            } else if (autoGenerate) {
                const dropTarget = document.getElementById(inputId);
                let container: HTMLElement | Document = dropTarget;
                let image, width = 0, height = 0;

                if (!noResize && _imageUtilAvailable && multiple) {
                    logDebug(formId, "Generating preview with ImageUtil.");
                    const { canvas, naturalWidth, naturalHeight } = await new ImageUtility(dataURL).resize(desiredWidth, desiredHeight, preserveAspectRatio);
                    image = this.addImageAttributes(dataURL, file, canvas, previewOptions, multiple, true, naturalWidth, naturalHeight);
                    width = naturalWidth;
                    height = naturalHeight;
                } else {
                    const tagImg = document.createElement("" + tagName) as HTMLImageElement;
                    image = this.addImageAttributes(dataURL, file, tagImg, previewOptions, multiple);
                }

                if (_isString(wrapperSelector)) {
                    if (wrapperSelector.startsWith("#")) {
                        // id specified, don't look for the element in the drop target
                        container = document.querySelector(wrapperSelector) as HTMLElement;
                    } else {
                        // query first the dropTarget
                        if (!!dropTarget) container = dropTarget.querySelector(wrapperSelector) as HTMLElement;

                        // then query the document if not found
                        if (!container) container = document.querySelector(wrapperSelector) as HTMLElement;
                    }

                    if (!container) {
                        // if an image container has been specified but not found, that's an error
                        logError(formId, `Wrapper ${wrapperSelector} for auto-generated image not found in the DOM tree.`);
                    }
                }

                if (!container) container = dropTarget || document;

                container.appendChild(image);

                return { width, height };

            } else {
                logWarning(formId, "Automatic image preview generation option is disabled!");
            }
        }

        return { width: 0, height: 0 };
    }

    /**
    * Add attributes for an auto-generated image.
    * @param {HTMLImageElement} img The target HTML element to add attributes to.
    * Can be any HTML element but usually it's an <img /> or a <canvas></canvas> tag.
    * @param {boolean} skipSrc true to skip setting the 'src' attribute; otherwise, false.
    * @param {number} naturalWidth Optional: The original width of the image.
    * @param {number} naturalHeight The original height of the image.
    */
    private addImageAttributes(dataURL: string, file: File, img: HTMLImageElement | HTMLCanvasElement, options: ImagePreviewOptions, multiple?: boolean, skipSrc?: boolean, naturalWidth?: number, naturalHeight?: number) {
        const fileSize = fileSizeToString(file.size);
        const { autoGeneratedClass, generateFileInfo, tagClass, src } = options;

        if (!skipSrc) img.setAttribute(src, dataURL);

        // use classList to preserve existing classes
        addClassList(img, tagClass);

        if (generateFileInfo) {
            /* A variation of what's built:
 
             <span class="form-manager-auto-generated">
                <img src="data:image;..."/>
                <span class="file-info">
                    <span class="name">file_name.jpg</span>
                    <span class="size">98.29 KB</span>
                </span>
                <span class="bg-dimmed"></span>
             </span>
             */
            const elcontainer = document.createElement("span");
            const elmeta = document.createElement("span");
            const elfilename = document.createElement("span");
            const elfilesize = document.createElement("span");
            const eldimmed = document.createElement("span");
            const eldimensions = naturalWidth && naturalHeight && document.createElement("span") || null;

            // add an internal class so that we can remove those created with the current script
            elcontainer.setAttribute("class", autoGeneratedClass);
            if (multiple) elcontainer.classList.add("gallery");

            elmeta.setAttribute("class", "file-info");
            elfilesize.setAttribute("class", "size");
            elfilename.setAttribute("class", "name");
            eldimensions && eldimensions.setAttribute("class", "dimensions");
            eldimmed.setAttribute("class", "bg-dimmed");

            elfilesize.textContent = fileSize;
            elfilename.textContent = file.name;
            eldimensions && (eldimensions.textContent = `${naturalWidth}x${naturalHeight}`);

            elmeta.appendChild(elfilesize);
            elmeta.appendChild(elfilename);
            eldimensions && elmeta.appendChild(eldimensions);

            elcontainer.appendChild(img);
            elcontainer.appendChild(elmeta);
            elcontainer.appendChild(eldimmed);

            return elcontainer;
        } else {
            addClassList(img, autoGeneratedClass).setAttribute("title", `${file.name} (${fileSize})`);
            return img;
        }
    }

    private defaultPreviewOptionsFrom(options: ImagePreviewOptions) {
        // delete invalid properties
        Object.keys(options).forEach(prop => {
            if (options[prop] === 0 || options[prop] === undefined || options[prop] === null)
                delete options[prop];
        });

        const def = DefaultImagePreviewOptions;
        const result = Object.assign({
            autoGenerate: def.autoGenerate,
            autoGeneratedClass: def.autoGeneratedClass,
            generateFileInfo: def.generateFileInfo,
            tagClass: def.tagClass,
            tagName: def.tagName,
            src: def.attributeName,
            wrapperSelector: def.wrapperSelector,
            width: def.width,
            height: def.height,
            preserveAspectRatio: def.preserveAspectRatio,
            noResize: def.noResize,
        }, options);

        return result;
    }

    /**
     * Remove auto-generated images.
     * @param {HTMLElement} container Optional: The HTML element that contains the images. Defaults to document.
     */
    removeAutoGeneratedImages(container) {
        const images = (container || document).querySelectorAll(`.${DefaultImagePreviewOptions.autoGeneratedClass}`);
        for (let i = 0; i < images.length; i++) {
            const img = images[i];
            img.remove();
        }
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
}

export {
    ImagePreviewGenerator,
    DefaultImagePreviewOptions
};