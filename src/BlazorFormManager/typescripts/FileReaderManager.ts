import { logDebug, logError, logWarning } from "./ConsoleLogger";
import { addEventHandler } from "./DomEventManager";
import { ImagePreviewGenerator } from "./ImagePreviewGenerator";
import { DragDropStorage, Forms } from "./Shared";
import { createFileInfo, removeFileList, supportsFileExtension, _isFunction, _isString } from "./Utils";

const _supportsFileReader = !!globalThis.FileReader;

/** A promise that resolves as a false boolean value. */
const CompletedPromise = new Promise<boolean>((resolve, _) => resolve(false));

const UPLOAD_EVENTS = { loadstart: 0, progress: 1, load: 2, error: 3, abort: 4, timeout: 5, loadend: 6 };

const READ_FILE_LIST_EVENTS = { start: 0, rejected: 1, processed: 2, end: 3 };
const READ_FILE_LIST_REJECTION = { none: 0, extension: 1, type: 2, multiple: 3, aborted: 4 };
const FILE_READER_FUNC = { 1: "readAsArrayBuffer", 2: "readAsBinaryString", 3: "readAsDataURL", 4: "readAsText", 5: "enumerate" };

export class FileReaderManager {
    private progressEvent = new SimpleEvent<any>();
    private _readFileListAborted = false;
    private previewGenerator: ImagePreviewGenerator;
    private _processedFileStorage: IDictionary<ProcessedFileConfig> = {};

    constructor(private interopInvoker: FormManagerInteropRef) {
    }

    get onprogress() {
        return this.progressEvent.expose();
    }

    /**
     * Set up event handlers for a FileReader object.
     * @param {string} formId The form identifier.
     * @param {FileReader} reader A FileReader object to which event listeners will be added.
     * @param {object} alternateHandlers Optional: An configuration options object to specify alternate event handlers.
     * Property names may be any of the following functions:
     * 'onstart' Event handler to invoke when the operation starts.
     * 'onprogress' Event handler to invoke the progress event. This handler should be capable of handling a Promise<boolean>.
     * 'onsuccess' Event handler to invoke when the operation completes successfully.
     * 'onfinish' Event handler to invoke when the operation finishes.
     * 'onabort' Event handler to invoke when the operation is aborted.
     * 'onerror' Event handler to invoke when an error occurs.
     */
    addFileReaderEventListeners(formId, reader, alternateHandlers) {
        const { onFileReaderChanged } = Forms[formId];
        if (!_isString(onFileReaderChanged)) return;

        this.addFileReaderOrUploadEventListeners(formId, reader, reader, onFileReaderChanged, true, alternateHandlers);
    }

    /**
     * Set up event handlers for an XMLHttpRequest.upload or FileReader object.
     * @param {string} formId The form identifier.
     * @param {XMLHttpRequest | FileReader} owner The object that owns the eventTarget (on which 'abort()' should be called if requested).
     * @param {object} eventTarget An object to which event listeners will be added.
     * If it's an XMLHttpRequest instance then it should its 'upload' property.
     * Otherwise, this is the same object has the 'owner'.
     * @param {string} interopCallback The name of the .NET callback function to invoke when a change occurs.
     * @param {boolean} hasFiles true if it's an upload operation with files; otherwise, false.
     * @param {object} alternateHandlers Optional: A configuration options object to specify alternate event handlers.
     * Property names may be any of the following functions:
     * 'onstart' Event handler to invoke when the operation starts.
     * 'onprogress' Event handler to invoke the progress event. This handler should be capable of handling a Promise<boolean>.
     * 'onsuccess' Event handler to invoke when the operation completes successfully.
     * 'onfinish' Event handler to invoke when the operation finishes.
     * 'onabort' Event handler to invoke when the operation is aborted.
     * 'onerror' Event handler to invoke when an error occurs.
     */
    addFileReaderOrUploadEventListeners(formId: string, owner: XMLHttpRequest | FileReader, eventTarget, interopCallback: string, hasFiles?: boolean, alternateHandlers?: any) {
        const me = this;
        /**
         * Handles a progress event.
         * @param {ProgressEvent<XMLHttpRequest>|ProgressEvent<FileReader>} e The event data.
         */
        async function handleEvent(e: ProgressEvent<XMLHttpRequest> | ProgressEvent<FileReader>) {
            const progressData = {
                bytesReadOrSent: e.loaded,
                totalBytesToReadOrSend: e.total,
                eventType: UPLOAD_EVENTS[e.type] | 0,
                hasFiles
            };

            if (me.progressEvent.anySubscriber())
                me.progressEvent.trigger({ formId, interopCallback, progressData });

            if (me.interopInvoker)
                return me.interopInvoker.invokeDotNet(formId, interopCallback, progressData);

            return CompletedPromise;
        }

        let { onstart, onprogress, onsuccess, onfinish, onabort, onerror } = alternateHandlers || {};

        if (!onstart) onstart = handleEvent;

        // The upload/read has begun.
        addEventHandler(eventTarget, 'loadstart', onstart);

        if (!onsuccess) onsuccess = handleEvent;

        // The upload/read completed successfully.
        addEventHandler(eventTarget, 'load', onsuccess);

        if (!onfinish) onfinish = handleEvent;

        /**
         * The upload/read finished. This event does not differentiate between
         * success or failure, and is sent at the end of the upload
         * regardless of the outcome. Prior to this event, one of load,
         * error, abort, or timeout will already have been delivered to
         * indicate why the upload ended.
         */
        addEventHandler(eventTarget, 'loadend', onfinish);

        if (!onprogress)
            onprogress = function (e) {
                handleEvent(e).then(cancel => (cancel) && owner.abort());
            };

        // Periodically delivered to indicate the amount of progress made so far.
        addEventHandler(eventTarget, 'progress', onprogress);

        if (!onerror) onerror = handleEvent;

        // The upload/read failed due to an error.
        addEventHandler(eventTarget, 'error', onerror);

        if (!onabort) onabort = handleEvent;

        // The upload/read operation was aborted.
        addEventHandler(eventTarget, 'abort', onabort);

        if ('ontimeout' in eventTarget) {
            // The upload timed out because a reply did not arrive within 
            // the time interval specified by the XMLHttpRequest.timeout.
            addEventHandler(eventTarget, 'timeout', handleEvent);
        }
    }


    /**
     * Read input files using the specified options
     * @param {{ formId: string, inputId: string, inputName: string, accept: string, acceptType: string, multiple: boolean, createObjectUrl: boolean, method: {readAsArrayBuffer: number, readAsBinaryString: number, readAsDataURL: number, readAsText: number, enumerate: number}, imagePreviewOptions: { autoGenerate: boolean, generateFileInfo: boolean, tagName: string, tagClass: string, tagId: string, attributeName: string, wrapperSelector: string, width: number, height: number, noResize: boolean, preserveAspectRatio: boolean }}}
     * options The file reader options.
     * @param {(files: FileList | File[]) => void} processedFileListCallback
     * A callback function that receives an array of the files effectively processed.
     */
    async readInputFiles(options: FileReaderOptions, processedFileListCallback: (files: FileList | File[]) => void) {
        logDebug(options.formId, "Preparing to read input files with options", options);

        const ERROR_CODE_BASE = 100;
        let succeeded = false, error = '', code = 0;

        if (!_supportsFileReader) {
            return { succeeded, error: "Your device does not support the FileReader API.", code: ERROR_CODE_BASE + 1 };
        }

        const { formId, method, inputId } = options;
        logDebug(formId, "Reading file(s) from input using options", options);

        // The .NET callback method name to invoke when the operation 
        // completes, unless the imagePreviewElement parameter is set.
        const { onFileReaderResult } = Forms[formId];

        if (FILE_READER_FUNC[method] === undefined) {
            succeeded = false;
            code = ERROR_CODE_BASE + 2;
            error = `Unsupported file reader method: ${method}`;
        } else if (_isString(inputId)) {

            if (!_isString(onFileReaderResult))
                logWarning(formId, "Managed callback 'onFileReaderResult' not specified.");

            const input = document.getElementById(inputId) as HTMLInputElement;

            if (input && 'files' in input) {
                return this.readFileList(input.files, options, processedFileListCallback);
            } else {
                code = ERROR_CODE_BASE + 4;
                error = `Specified input id #${inputId} does not identify an input element of type 'file'.`;
            }
        } else {
            code = ERROR_CODE_BASE + 3;
            error = `Invalid file reader options: 'inputId' not present.`;
        }

        logError(formId, error);
        succeeded = false;
        return { succeeded, error, code };
    }

    enforceMultipleFilesPolicy(formId: string, targetId: string, fileCount: number) {
        if (fileCount > 1)
            logWarning(null, `Dropping multiple files (${fileCount}) here is not allowed. Picking first and ignoring the rest.`);

        fileCount = this.dragDropGetFileCount(targetId);
        logDebug(formId, "dragDropGetFileCount", fileCount);

        // remove existing files
        if (fileCount > 0)
            removeFileList({ formId, targetId });
    }

    /**
     * Read a list of files.
     * @param files The list of files to read.
     * @param options An object that encapsulates settings for file reading operations.
     * @param processedFileListCallback A callback function that receives an array of the files effectively processed.
     * @param storeOnly true to validate file acceptance and store files without reading them; otherwise, false.
     */
    async readFileList(files: FileList | File[], options: FileReaderOptions, processedFileListCallback: (files: FileList | File[]) => void, storeOnly?: boolean) {
        const ERROR_CODE_BASE = 200;
        const fileList = files instanceof Array ? files : Array.from(files);
        const fileCount = fileList && fileList.length || 0;

        if (fileCount > 0) {
            const { formId, accept, acceptType, multiple, inputId, createObjectUrl } = options;

            // check for multiple files
            if (!multiple) this.enforceMultipleFilesPolicy(formId, inputId, fileCount);

            const hasAccept = _isString(accept);
            const hasAcceptType = _isString(acceptType);
            const acceptAllFiles = !(hasAccept || hasAcceptType);
            const supportedFiles = acceptAllFiles
                ? []
                : hasAccept ? (accept + '').split(',').map(type => type.trim().toLowerCase()) : undefined;

            const processedFileList = [];
            const { onReadFileList } = Forms[formId];
            const hasCallback = _isFunction(processedFileListCallback);
            const hasOnReadFileList = _isString(onReadFileList);
            const abortedByUser = "File list reading operation has been aborted by user";

            let notAllowed = false;
            let dimensions = { width: 0, height: 0 };

            this._readFileListAborted = false;

            if (acceptAllFiles) logDebug(formId, "Any file accepted.");

            if (hasOnReadFileList)
                await this.interopInvoker.invokeDotNet(formId, onReadFileList, { type: READ_FILE_LIST_EVENTS["start"], totalFilesToRead: fileCount });

            for (let i = 0; i < fileCount; i++) {
                logDebug(formId, `Processing file... ${i + 1} of ${fileCount}`);

                const file = fileList[i];

                if (!acceptAllFiles) {
                    if (hasAccept) {
                        if (!supportsFileExtension(file.name, supportedFiles)) {
                            logWarning(formId, `File "${file.name}" is not allowed.`);
                            notAllowed = true;

                            if (hasOnReadFileList) {
                                const evArgs = {
                                    type: READ_FILE_LIST_EVENTS["rejected"],
                                    reason: READ_FILE_LIST_REJECTION["extension"],
                                    file: createFileInfo(file, null, createObjectUrl),
                                    filesRead: i + 1,
                                    totalFilesToRead: fileCount
                                };
                                const cancel = await this.interopInvoker.invokeDotNet(formId, onReadFileList, evArgs);
                                if (cancel) {
                                    this._readFileListAborted = true;
                                    logDebug(formId, `${abortedByUser} after rejected file extension.`);
                                    break;
                                }
                            }

                            continue;
                        }
                    }

                    if (hasAcceptType) {
                        // checking could be more elaborated using regex but for now it'll do it
                        if (file.type && file.type.indexOf(acceptType) === -1) {
                            logWarning(formId, `File "${file.name}" of type ${file.type} is not allowed.`);
                            notAllowed = true;

                            if (hasOnReadFileList) {
                                const evArgs = {
                                    type: READ_FILE_LIST_EVENTS["rejected"],
                                    reason: READ_FILE_LIST_REJECTION["type"],
                                    file: createFileInfo(file, null, createObjectUrl),
                                    filesRead: i + 1,
                                    totalFilesToRead: fileCount
                                };
                                const cancel = await this.interopInvoker.invokeDotNet(formId, onReadFileList, evArgs);
                                if (cancel) {
                                    this._readFileListAborted = true;
                                    logDebug(formId, `${abortedByUser} after rejected file type.`);
                                    break;
                                }
                            }

                            continue;
                        }
                    }
                }

                dimensions = null;

                if (!storeOnly) {
                    logDebug(formId, `Reading file ${file.name}`);

                    const result = await this.readFileCore(
                        file,
                        options,
                        async dataURL => {
                            if (!this.previewGenerator) {
                                this.previewGenerator = new ImagePreviewGenerator();
                            }
                            dimensions = await this.previewGenerator.generateImagePreview(dataURL, file, options);
                        }
                    );

                    if (result.aborted) {
                        this._readFileListAborted = true;
                        break;
                    }
                }

                if (hasOnReadFileList) {
                    const evArgs = {
                        type: READ_FILE_LIST_EVENTS["processed"],
                        file: createFileInfo(file, dimensions, createObjectUrl),
                        filesRead: i + 1,
                        totalFilesToRead: fileCount
                    };
                    const cancel = await this.interopInvoker.invokeDotNet(formId, onReadFileList, evArgs);
                    if (cancel) {
                        if (cancel === 5)
                            continue; // rejection by application policy (file ignored)

                        this._readFileListAborted = true;
                        logDebug(formId, abortedByUser);
                        break;
                    }
                }

                if (hasCallback)
                    processedFileList.push(file);

                if (this._readFileListAborted)
                    break;

                if (!multiple) {
                    if (fileCount > 1) {
                        logWarning(formId, `Processing multiple files (${fileCount}) is not allowed. The remaining files are ignored.`);

                        if (hasOnReadFileList) {
                            const evArgs = {
                                type: READ_FILE_LIST_EVENTS["rejected"],
                                reason: READ_FILE_LIST_REJECTION["multiple"],
                                files: [...fileList].slice(i + 1).map(f => createFileInfo(f, null, createObjectUrl)),
                                filesRead: i + 1,
                                totalFilesToRead: fileCount
                            };
                            await this.interopInvoker.invokeDotNet(formId, onReadFileList, evArgs);
                        }
                    }
                    break;
                }
            }

            if (this._readFileListAborted)
                logDebug(formId, "Exited 'readFileList' loop prematurely!");

            const filesRead = processedFileList.length;

            if (!multiple && notAllowed)
                this.resetInputFile(formId, options.inputFileId);
            if (hasCallback && filesRead && !this._readFileListAborted)
                processedFileListCallback.call(this, processedFileList);

            if (hasOnReadFileList) {
                await this.interopInvoker.invokeDotNet(formId, onReadFileList, {
                    type: READ_FILE_LIST_EVENTS["end"],
                    reason: this._readFileListAborted ? READ_FILE_LIST_REJECTION["aborted"] : READ_FILE_LIST_REJECTION["none"],
                    filesRead,
                    totalFilesToRead: fileCount
                })
            }

            this._readFileListAborted = false;
            return { succeeded: true, error: null, code: 0 };
        } else {
            const error = "No file available to read.";
            logDebug(options.formId, error);
            return { succeeded: false, error, code: ERROR_CODE_BASE + 1 };
        }
    }

    /**
     * Read a file.
     * @param {File} file
     * @param {any} options
     * @param {Promise<void>} setDataURLCallback
     */
    async readFileCore(file: File, options, setDataURLCallback: (dataURL: string) => Promise<void>) {
        const { formId, method, inputId = null, inputName = null } = options;
        const { onFileReaderResult } = Forms[formId];
        const methodName = FILE_READER_FUNC[method];
        const result = {
            // given back these 3 properties makes the identification
            // in the 'onFileReaderResult' callback more accurate
            method: method + "",
            inputId: inputId + "",
            inputName: inputName + "",

            aborted: false,
            succeeded: false,
            completedInScript: false,
            error: "",
            content: "",
            contentArray: [],
        };

        try {
            if (methodName === "enumerate") {
                logDebug(formId, "Enumerating file ", file);
                result.succeeded = true;
            } else {
                // create a Promise that takes care of reading the file
                const promiseResult = await this.createFileReaderPromise(formId, file, method);
                const { succeeded, aborted, content } = promiseResult;

                if (succeeded) {
                    if (methodName === "readAsArrayBuffer") {
                        // convert to a 'normal' array
                        const contentArray = Array.prototype.slice.call(new Uint8Array(content as ArrayBuffer));
                        result.contentArray = contentArray;

                    } else if (_isFunction(setDataURLCallback) &&
                        methodName === "readAsDataURL" &&
                        file.type && file.type.indexOf("image") !== -1) {

                        await setDataURLCallback.call(this, content);
                        result.completedInScript = true;
                    } else {
                        result.content = content as string;
                    }
                }

                result.succeeded = succeeded;
                result.aborted = !!aborted;
            }

            if (_isString(onFileReaderResult))
                await this.interopInvoker.invokeDotNet(formId, onFileReaderResult, result);

            // clean up
            if (!result.completedInScript) {
                result.content = null;
                result.contentArray = null;
            }

            return result;
        } catch (e) {
            const error = e.message || e;
            result.succeeded = false;
            result.error = error + "";

            logError(formId, e);

            if (_isString(onFileReaderResult)) await this.interopInvoker.invokeDotNet(formId, onFileReaderResult, result);

            return result;
        }
    }

    /**
     * Remove from the processed file storage the files 
     * that have been previously been successfully selected.
     * @param {{formId: string; inputId?: string}} options
     */
    deleteProcessedFileList(options: { formId: string; inputId?: string }) {
        logDebug("Deleting processed files with options: ", options);

        const { formId, inputId } = options || {};
        const config = this._processedFileStorage[formId];

        if (config) {
            const inputs = config.inputs;

            if (inputId && inputs) {
                const input = inputs.find(inp => inp.id === inputId);
                if (!!input) {
                    delete input.id;
                    delete input.name;
                    delete input.files;
                }
            }

            // remove the entire storage for the specified form
            delete this._processedFileStorage[formId];
            logDebug("Deleted processed file storage for form #" + formId);
            return true;
        } else {
            logDebug("No processed files associated with form #" + formId);
        }

        return false;
    }

    /**
     * Create a Promise that will handle all supported file reading events and notifications to the Blazor app.
     * @param {string} formId The form identifier.
     * @param {File} file The file to read.
     * @param {number} method An integer between 1 and 4.
     * @return {Promise<{succeeded: boolean, aborted: boolean, content: string|ArrayBuffer}>}
     */
    private createFileReaderPromise(formId, file, method): Promise<{ succeeded: boolean, aborted: boolean, content: string | ArrayBuffer }> {
        const methodName = FILE_READER_FUNC[method];
        const { onFileReaderChanged } = Forms[formId];
        const me = this;

        return new Promise((resolve, reject) => {
            try {
                let succeeded = false;
                let aborted = false;
                const reader = new FileReader();

                // custom event handlers
                const onstart = e => raiseNotification(e);

                const onprogress = async e => {
                    const cancel = await raiseNotification(e);
                    if (cancel) {
                        // Phew! This one was hard to debug...!
                        // The local 'aborted' flag isn't enough for a small-sized file;
                        // it may have already been read to end after the notification returns,
                        // hence the 'readFileList' function won't get a chance to quit because
                        // the 'aborted' property isn't set in the resolved object.

                        // That's why we also set the '_readFileListAborted' field to true in
                        // case the current promise was created in the call-chain of 'readFileList'.
                        this._readFileListAborted = aborted = true;

                        reader.abort();
                        logWarning(formId, "File reading operation was aborted on demand.");
                    }
                };

                // onload
                const onsuccess = e => {
                    succeeded = !aborted;
                    return raiseNotification(e);
                };

                // onloadend
                const onfinish = async e => {
                    await raiseNotification(e);
                    resolve({ succeeded, aborted, content: aborted ? null : e.target.result });
                };

                const onerror = async e => {
                    const error = `Failed to read file! Reason: ${reader.error}`;
                    logDebug(formId, error);
                    await raiseNotification(e, error);
                    reject(error);
                };

                const onabort = async () => {
                    this._readFileListAborted = aborted = true;
                };

                const alternateHandlers = { onstart, onprogress, onsuccess, onfinish, onerror, onabort };
                this.addFileReaderEventListeners(formId, reader, alternateHandlers);

                logDebug(formId, "Attempting to read file using method", methodName);

                switch (methodName) {
                    case "readAsArrayBuffer":
                        reader.readAsArrayBuffer(file);
                        break;
                    case "readAsBinaryString":
                        reader.readAsBinaryString(file);
                        break;
                    case "readAsDataURL":
                        reader.readAsDataURL(file);
                        break;
                    case "readAsText":
                        reader.readAsText(file);
                        break;
                    default:
                        reject(new Error(`Unsupported method: ${method}`));
                }
            } catch (e) {
                logDebug(formId, "Error reading file", e);
                reject(e);
            }
        });

        /**
         * Invoke .NET method when a progress event is received.
         * @param {ProgressEvent<FileReader>} e The event data.
         * @param {string} error Optional: A message that describes the error that might have occurred.
         */
        async function raiseNotification(e: ProgressEvent<FileReader>, error?: any) {
            const eventType = UPLOAD_EVENTS[e.type] | 0;
            const eventData = {
                bytesReadOrSent: e.loaded,
                totalBytesToReadOrSend: e.total,
                eventType,
                hasFiles: true,
                error: error || null
            };
            if (eventType === 0) eventData['file'] = createFileInfo(file);
            return me.interopInvoker.invokeDotNet(formId, onFileReaderChanged, eventData);
        }
    }

    /**
     * Attempt to set the value of an input file to an empty string.
     * @param {string} formId The form identifier.
     * @param {string} inputFileId The identifier of the input file to reset.
     */
    resetInputFile(formId: string, inputFileId: string) {
        logDebug(formId, `Resetting input file #${inputFileId}`);
        try {
            const input = document.getElementById(inputFileId) as HTMLInputElement;
            input && (input.value = "");
        } catch (e) {
            logError(formId, `Error resetting the value of file input #${inputFileId}`, e);
        }
    }

    /**
     * Return the number of files dropped on the identified target.
     * @param {string} dropTargetId
     */
    private dragDropGetFileCount(dropTargetId) {
        const config = DragDropStorage[dropTargetId];
        return (!!config && !!config.droppedFiles && config.droppedFiles.fileCount | 0) || 0;
    }
}
