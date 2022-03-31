/*
 Name:          ImageUtil
 Version:       0.1.0
 Author:        Abdourahamane Kaba
 Description:   A utility class that provides methods to crop, resize, and perform other image transformation algorithms.
 License:       Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 Copyright:     © Karfamsoft. All rights reserved.
 */
(function (global) {
    /**
     * Initializes a new instance of the ImageUtil class.
     * @param {string|Blob} source The image's source. Can be a URI, a base64 data URL, or a file.
     */
    const ImageUtil = function ImageUtil(source) {
        if (typeof (source) === "string") {
            /** The image's source (URL or base64 data URL). */
            this.url = source;
        } else if (source instanceof Blob) {
            readFile(source, data => this.url = data);
        } else {
            throw new Error(`Source type ${typeof (source)} not supported.`);
        }
    }

    /**
     * Crop an image to desired aspect ratio.
     * @param {number} aspectRatio The aspect ratio
     */
    ImageUtil.prototype.crop = function (aspectRatio) {
        /**
        * Image crop handler.
        * @param {HTMLImageElement} sourceImage The source image element.
        */
        const handler = sourceImage => {
            const sourceWidth = sourceImage.naturalWidth;
            const sourceHeight = sourceImage.naturalHeight;
            const sourceAspectRatio = sourceWidth / sourceHeight;
            let targetWidth = sourceWidth;
            let targetHeight = sourceHeight;

            if (sourceAspectRatio > aspectRatio) {
                targetWidth = sourceHeight * aspectRatio;
            } else if (sourceAspectRatio < aspectRatio) {
                targetHeight = sourceWidth / aspectRatio;
            }

            const targetX = (targetWidth - sourceWidth) * .5;
            const targetY = (targetHeight - sourceHeight) * .5;
            const canvas = document.createElement('canvas');

            canvas.width = targetWidth;
            canvas.height = targetHeight;
            canvas.getContext('2d').drawImage(sourceImage, targetX, targetY);

            return canvas;
        };

        return createImage(this.url, handler);
    };

    /**
     * Resize an image by preserving its aspect ratio.
     * @param {number} targetWidth Optional: The desired width of the resulting image. If not set, targetHeight must be set.
     * @param {number} targetHeight Optional: The desired height of the resulting image. If not set, targetWidth must be set.
     * @param {boolean} preserveAspectRatio Optional: Indicates whether to keep the original aspect ratio.
     */
    ImageUtil.prototype.resize = function (targetWidth, targetHeight, preserveAspectRatio) {

        if (!(targetWidth || targetHeight))
            throw new Error("At least one of the image dimensions must be set.");
        if (targetWidth < 0 || targetHeight < 0)
            throw new Error("Image dimensions cannot be negative.");

        /**
         * Image resizing handler.
         * @param {HTMLImageElement} sourceImage The source image element.
         */
        const handler = sourceImage => {
            /* 
             * sourceRatio = sourceWidth / sourceHeight
             * targetRatio = targetWidth / targetHeight
             * sourceRatio == targetRatio => sourceWidth / sourceHeight == targetWidth / targetHeight;
             * 
             * Knowing either targetWidth or targetHeight:
             * 
             * targetWidth = sourceWidth * targetHeight / sourceHeight;
             * targetHeight = sourceHeight * targetWidth / sourceWidth;
            */
            const sourceWidth = sourceImage.naturalWidth;
            const sourceHeight = sourceImage.naturalHeight;

            if (!!preserveAspectRatio) {
                if (targetWidth > 0)
                    targetHeight = sourceHeight * targetWidth / sourceWidth;
                else
                    targetWidth = sourceWidth * targetHeight / sourceHeight;
            }

            const canvas = document.createElement('canvas');

            canvas.width = targetWidth;
            canvas.height = targetHeight;
            canvas.getContext('2d').drawImage(sourceImage, 0, 0, targetWidth, targetHeight);

            return canvas;
        };

        return createImage(this.url, handler);
    };

    /**
     * Create an HTMLImageElement, load and manipulate it using the specified handler.
     * @param {string} src The source URL of the image to create.
     * @param {(source: HTMLImageElement) => HTMLCanvasElement} handler An image manipulation callback function.
     * @return {Promise<{canvas: HTMLCanvasElement, naturalWidth: number, naturalHeight: number}>} A Promise that resolves with the resulting HTMLCanvasElement.
     */
    function createImage(src, handler) {
        return new Promise(resolve => {
            const sourceImage = new Image();
            sourceImage.onload = function () {
                resolve({
                    canvas: handler.call(this, sourceImage),
                    naturalWidth: sourceImage.naturalWidth,
                    naturalHeight: sourceImage.naturalHeight
                })
            };
            sourceImage.src = src;
        });
    }

    /**
     * Read the specified source using an instance of FileReader.
     * @param {Blob} source The file or other blob-like object to read.
     * @param {(dataURL: string) => void} dataCallback
     */
    function readFile(source, dataCallback) {
        const reader = new FileReader();

        /**
         * FileReader's 'load' event handler.
         * @param {ProgressEvent<FileReader>} e
         */
        const handler = e => dataCallback.call(e, e.target.result);

        if (!!reader.attachEvent) {
            reader.attachEvent('onload', handler);
        } else {
            reader.addEventListener("load", handler);
        }

        reader.readAsDataURL(source);
    }

    global.ImageUtil = ImageUtil;
})(window);
