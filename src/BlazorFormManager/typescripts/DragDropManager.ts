import { logDebug, logError } from "./ConsoleLogger";
import { addEventHandler, getMouseEventArgs } from "./DomEventManager";
import { FileReaderManager } from "./FileReaderManager";
import { DragDropStorage, Forms, SIZE_MB } from "./Shared";
import { createFileInfo, fileSizeToString, notFound, _isObject, _isString } from "./Utils";

const DROP_EFFECTS = { none: 0, copy: 1, link: 2, move: 3 };
const DROP_EFFECTS_ALLOWED = { none: 0, copy: 1, copyLink: 2, copyMove: 3, link: 4, linkMove: 5, move: 6, all: 7, uninitialized: 8 };

export class DragDropManager {

    constructor(private fileManager: FileReaderManager, private interopInvoker: FormManagerInteropRef) {
    }

    /**
     * Enable drag and drop support for the target element.
     * @param {any} options An object that contains at least these 3 properties: 'dropTargetId', 'dropEffect', and 'inputName'.
     */
    async enable(options) {
        const ERROR_CODE_BASE = 400;
        const {
            formId,
            dropTargetId,
        } = options;

        logDebug(formId, "Enable drag and drop support with options", options);

        let succeeded = false, error = '', code = 0;
        const targetId = "" + dropTargetId;

        if (targetId in DragDropStorage) {
            code = ERROR_CODE_BASE + 1;
            error = `The drop target element identified by #${targetId} has already been registered.`;
            logDebug(formId, error);
        } else {
            const dropTarget = document.getElementById(targetId);

            if (dropTarget) {
                const { dropEffect = "copy", effectAllowed = "copy" } = options;

                /**
                 * A drag operation has started.
                 * @param {DragEvent} e
                 */
                const dragstart = async (e: DragEvent) => {
                    e.stopPropagation();
                    e.preventDefault();

                    let effectSet = false;
                    const dt = e.dataTransfer;

                    logDebug(formId, `dragStart: dropEffect = ${dt.dropEffect} ; effectAllowed = ${dt.effectAllowed}`);

                    // Since FormsOptions can be updated, let's get the most current values
                    // of 'onDragStart' and 'onDrop' every time a drag and drop operation occurs.
                    const { onDragStart } = Forms[formId];

                    if (_isString(onDragStart)) {
                        const args = getMouseEventArgs(e);

                        args['dataTransfer'] = {
                            dropEffect: dt.dropEffect,
                            effectAllowed: dt.effectAllowed,
                            files: [],
                            items: [...Array.from(dt.items)].map(i => ({ kind: i.kind, type: i.type })),
                            types: dt.types
                        };

                        const result = await this.interopInvoker.invokeDotNet(formId, onDragStart, args) as DragEventResponse;

                        if (_isObject(result)) {
                            const { data, dataFormat, effectAllowed: effect } = result;

                            if (_isString(data) && _isString(dataFormat)) {
                                dt.setData(dataFormat, data);
                                logDebug(formId, `Drag start event data set. Format=${dataFormat}`, data);
                            }

                            if (_isString(effect)) {
                                dt.effectAllowed = effect;
                                effectSet = true;
                            }
                        }
                    }

                    if (!effectSet) dt.effectAllowed = effectAllowed;
                };

                /**
                 * Handles the 'dragover' event.
                 * @param {DragEvent} e
                 */
                const dragover = (e: DragEvent) => {
                    e.stopPropagation();
                    e.preventDefault();
                    e.dataTransfer.dropEffect = dropEffect;
                };

                // copy the options used for dropping so that they can 
                // be locally overriden without global consequences
                const dropOptions = Object.assign({}, options);

                const drop = e => {
                    this.handleDrop(e, dropOptions, dropTarget);
                };

                addEventHandler(dropTarget, 'dragstart', dragstart);
                addEventHandler(dropTarget, 'dragover', dragover);
                addEventHandler(dropTarget, 'drop', drop);

                // required to clean up the mess on demand
                DragDropStorage[targetId] = {
                    dragstart,
                    dragover,
                    drop,
                    options
                };

                succeeded = true;
                error = null;
                logDebug(formId, `Drag and drop enabled for target element #${targetId}.`);
            } else {
                code = ERROR_CODE_BASE + 2;
                error = notFound(targetId, "drop target");
            }
        }

        return { succeeded, error, code };
    }

    /**
     * Clean up by removing drag and drop event listeners.
     * @param {string} targetId The drop target element identifier.
     */
    disable({ formId, targetId }) {
        const ERROR_CODE_BASE = 500;
        // explicitly initialize these variables to 
        // make the return type of the function clear
        let succeeded = false, error = '', code = 0;

        if (targetId in DragDropStorage) {
            const dropTarget = document.getElementById(targetId);

            if (!!dropTarget) {
                const config = DragDropStorage[targetId];

                dropTarget.removeEventListener("dragstart", config.dragstart);
                dropTarget.removeEventListener("dragover", config.dragover);
                dropTarget.removeEventListener("drop", config.drop);

                delete config.dragstart;
                delete config.dragover;
                delete config.drop;
                delete config.options;
                if ('droppedFiles' in config) delete config.droppedFiles;

                error = null;
                succeeded = true;
                logDebug(formId, `Drag and drop disabled for #${targetId}.`);
            } else {
                code = ERROR_CODE_BASE + 2;
                error = notFound(targetId, "drop target");
            }

            delete DragDropStorage[targetId];

        } else {
            code = ERROR_CODE_BASE + 1;
            error = `The event listeners for the drop target element #${targetId} ` +
                "do not exist or have already been removed.";
            logDebug(formId, error);
        }

        return { succeeded, error, code };
    }

    /**
     * Read files from an input identified by 'options.inputId' and add them to the 
     * temporary drag and drop storage identified by 'options.dropTargetId'.
     * @param {any} options An object containing the properties 'targetId',
     * 'inputId', 'inputName', and 'method'.
     */
    async dragDropInputFilesOnTarget(options: TargetDropOptions) {
        const ERROR_CODE_BASE = 600;
        let succeeded = false, error = '', code = 0;
        const { formId, targetId, inputId, inputName, method } = options;

        // first, check if the drop target settings exist
        const config = DragDropStorage[targetId];

        if (!config) {
            code = ERROR_CODE_BASE + 1;
            error = `Drag and drop settings not found for element #${targetId}.`;
            logError(formId, error);
            return { succeeded, error, code };
        } else {
            // second, get a copy of the stored options from config
            const newOptions = Object.assign({}, config.options);

            // and set new properties appropriately
            newOptions.inputId = inputId;
            newOptions.method = method;

            // finally, read the files and store the processed list into the drag and drop settings
            return await this.fileManager.readInputFiles(newOptions, processedFiles => {
                this.dragDropAfterFilesProcessed(formId, processedFiles, targetId, inputName);
            });
        }
    }

    /**
     * Add the list of files to the drag and drop store.
     * @param {string} formId The form identifier.
     * @param {File[]} files The list of files to add.
     * @param {string} dropTargetId The identifier of the drop target element.
     * @param {string} inputName The name of the form field associated with the files.
     * @param {HTMLElement} dropTargetElement Optional: An HTML element that represents the drop target.
     * If not specified, 'dropTargetId' is used to retrieve it in the DOM. It is used to remove
     * the CSS class 'drag-drop-area-empty' from it.
     * @param {boolean} keepEmtpyClass true to keep the 'drag-drop-area-empty' class on the drop target.
     */
    dragDropAfterFilesProcessed(formId: string, files: FileList | File[], dropTargetId: string, inputName?: string, dropTargetElement?: HTMLElement, keepEmtpyClass?: boolean) {
        if (files && files.length) {
            // store the dropped files for later use when submitting the form
            const config = DragDropStorage[dropTargetId];
            if (!!config) {
                // make sure to add to any previously-dropped files
                const { droppedFiles } = config;
                if (!!droppedFiles) {
                    const existingFiles = droppedFiles[inputName];
                    if (existingFiles instanceof Array)
                        files = existingFiles.concat(files);
                }
                config.droppedFiles = { [inputName]: files, fileCount: files.length };
                logDebug(formId, `Dropped files stored for target #${dropTargetId} and input ${inputName}.`);
                if (!keepEmtpyClass) {
                    dropTargetElement || (dropTargetElement = document.getElementById(dropTargetId));
                    dropTargetElement && dropTargetElement.classList.remove("drag-drop-area-empty");
                }
            } else {
                logError(formId, `No drop store found for target #${dropTargetId}.`);
            }
        }
    }

    /**
     * Collect all files dropped so far.
     * @param formId The form identifier.
     */
    collectFiles(formId: string) {
        logDebug(formId, "Collecting drag/drop files...");
        const collectedFiles: Array<{ name: string; value: File }> = [];
        const dds = DragDropStorage;

        for (const dropTargetId in dds) {
            if (dds.hasOwnProperty(dropTargetId)) {
                const config = dds[dropTargetId];
                const { droppedFiles } = config;

                if (!!droppedFiles && droppedFiles.fileCount > 0) {
                    for (const inputName in droppedFiles) {
                        if (droppedFiles.hasOwnProperty(inputName)) {
                            const fileList = droppedFiles[inputName];

                            for (let i = 0; i < fileList.length; i++) {
                                collectedFiles.push({
                                    name: inputName,
                                    value: fileList[i]
                                });
                            }
                        }
                    }
                } else {
                    logDebug(formId, `No file dropped onto target #${dropTargetId}.`);
                }
            }
        }

        logDebug(formId, `Collected ${collectedFiles.length} drag and drop file(s) for target elements.`);
        return collectedFiles;
    }

    /**
     * Handles the 'drop' event.
     * @param {DragEvent} e
     * @param {any} options
     * @param {HTMLElement} dropTarget
     */
    private async handleDrop(e: DragEvent, options: FileReaderOptions, dropTarget: HTMLElement) {
        e.stopPropagation();
        e.preventDefault();

        const { formId, inputFileId, multiple, inputName, createObjectUrl } = options;
        const dt = e.dataTransfer;
        const fileList = Array.from(dt.files);
        const fileCount = fileList && fileList.length || 0;
        const hasFiles = fileCount > 0;
        const files = hasFiles ? [...fileList] : [];
        const { onDrop: onDropInterop } = Forms[formId];

        let filesToProcess = files;
        let storeFilesOnly = false;

        logDebug(formId, "Processing dropped files with options", options);

        if (hasFiles) {
            if (!multiple) filesToProcess = [files[0]];

            logDebug(formId, `${fileCount} file(s) dropped onto target element #${dropTarget.id}`);
            if (_isString(inputFileId)) this.fileManager.resetInputFile(formId, inputFileId);
        }

        if (_isString(onDropInterop)) {
            const args = {
                dataTransfer: {
                    dropEffect: DROP_EFFECTS[dt.dropEffect],
                    effectAllowed: DROP_EFFECTS_ALLOWED[dt.effectAllowed],
                    files: hasFiles ? files.map(f => createFileInfo(f, null, createObjectUrl)) : [],
                    items: dt.items ? [...Array.from(dt.items)].map(dti => ({ kind: dti.kind, type: dti.type })) : [],
                    types: dt.types,
                },
                targetId: dropTarget.id,
                multiple: !!multiple,
            };

            logDebug(formId, "Drop event args:", args);

            const result = await this.interopInvoker.invokeDotNet(formId, onDropInterop, args) as DragEventResponse;

            logDebug(formId, "Drag event response:", result);

            if (_isObject(result)) {
                const { cancel, acceptedFiles, storeOnly, maxFileCount, maxTotalSize, imagePreviewOptions } = result;

                if (cancel) {
                    logDebug(formId, `Drop operation has been cancelled by the user/app.`);
                    return;
                }

                if (hasFiles) {
                    if (acceptedFiles instanceof (Array) && acceptedFiles.length) {
                        // process only specific files

                        logDebug(formId, "Filtering out specific files to be processed defined by the application.");

                        // cast acceptedFiles from 'any' to 'string' for strongly-typed file name equality comparison
                        const fileNamesAsStringArray = acceptedFiles.map(n => "" + n);

                        // find corresponding items in the original 'files' array
                        filesToProcess = fileNamesAsStringArray
                            .map(name => files.find(file => file.name === name)) // type- and case-sensitive comparison
                            .filter(f => !!f); // remove potentially undefined items returned by the 'map' function
                    }

                    // only integers allowed
                    const maxcount = maxFileCount | 0;

                    if (maxcount > 0) {
                        filesToProcess = filesToProcess.slice(0, maxcount);
                        logDebug(formId, `Maximum ${maxcount} of ${fileCount} files selected for processing.`);
                    }

                    if (maxTotalSize > 0) {
                        logDebug(`Limiting total file size to ${maxTotalSize} MB.`);

                        let sum = 0;
                        const totalFiles = []
                        // convert max from MB to bytes
                        const maxTotalSizeBytes = SIZE_MB * maxTotalSize;

                        for (let i = 0; i < filesToProcess.length; i++) {
                            const file = filesToProcess[i];
                            if ((sum + file.size) <= maxTotalSizeBytes) {
                                sum += file.size;
                                totalFiles.push(file);
                            } else {
                                break;
                            }
                        }

                        const initialFileCount = filesToProcess.length;
                        filesToProcess = totalFiles;

                        logDebug(formId, `${filesToProcess.length} of ${initialFileCount} files ` +
                            `selected for processing.Total size: ${fileSizeToString(sum)}`);
                    }

                    storeFilesOnly = storeOnly;
                    if (_isObject(imagePreviewOptions))
                        options.imagePreviewOptions = imagePreviewOptions;
                }
            }
        }

        // make sure that files have been dropped onto the target
        if (hasFiles && filesToProcess.length) {
            logDebug(formId, `Processing ${filesToProcess.length} dropped file(s)...`);

            /**
             * Callback invoked when at least one file has been effectively processed.
             * @param {File[]} processedFileList
             */
            const cb = processedFileList => {
                const dropTargetId = dropTarget.id;
                if (storeFilesOnly) dropTarget = undefined;
                this.dragDropAfterFilesProcessed(formId, processedFileList, dropTargetId, inputName, dropTarget, /*keepEmptyClass:*/ storeFilesOnly);
            };

            await this.fileManager.readFileList(filesToProcess, options, cb, storeFilesOnly);
        }
    }

    /**
     * Return the number of files dropped on the identified target.
     * @param {string} dropTargetId
     */
    dragDropGetFileCount(dropTargetId) {
        const config = DragDropStorage[dropTargetId];
        return (!!config && !!config.droppedFiles && config.droppedFiles.fileCount | 0) || 0;
    }
}