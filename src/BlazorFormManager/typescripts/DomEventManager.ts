import { logDebug, logError } from "./ConsoleLogger";
import { populateDictionary, _isString, _isObject, notFound } from "./Utils";
import { Forms, AssemblyName } from "./Shared";

const FILTER_KEY_TYPES = { none: 0, alpha: 1, alphaNumeric: 2, digits: 3 };
const dotnet = globalThis.DotNet;
const Listeners: IDictionary<EventListenerOrEventListenerObject> = {};

const addListener = function (targetId: string, eventType: string, listener: EventListenerOrEventListenerObject) {
    const key = targetId + eventType;
    Listeners[key] = listener;
}

const removeListener = function (targetId: string, eventType: string) {
    const key = targetId + eventType;
    if (key in Listeners)
        delete Listeners[key];
}

/** Provides static methods for defining and handling events. */
class DomEventManager {

    /**
     * Add an event listener to a specified target element identified by 'targetId'.
     * @param targetId The identifier of the target element.
     * @param eventType The type of event to add the listener for.
     * @param callback The static .NET method to invoke when an event occurs.
     */
    static addEventListener(targetId: string, eventType: string, callback?: string) {
        logDebug(null, `Adding '${eventType}' event listener for #${targetId}.`);
        const target = document.getElementById(targetId);

        if (target) {
            const callbackDefined = dotnet && _isString(callback);

            /**
             * The event listener for the element.
             * @param event The event data.
             */
            const listener = function (event: Event) {
                const obj = {
                    targetId,
                    eventType,
                    arguments: populateDictionary({}, event),
                    value: target['value'] === undefined ? "" : target['value']
                };
                // logDebug(`Event received: ${event.type}, invoking .NET method ${callback}...`, obj);
                if (callbackDefined)
                    dotnet.invokeMethodAsync(AssemblyName, callback, obj);
            };

            if (DomEventManager.addEventHandler(target, eventType, listener)) {
                logDebug(null, "Event listener added successfully.");
            }

            return true;
        } else {
            notFound(targetId);
            return false;
        }
    }

    /**
     * Remove an event listener from a target element identified by 'targetId'.
     * @param targetId The identifier of the target element.
     * @param eventType The type of event to remove the listener for.
     */
    static removeEventListener(targetId: string, eventType: string) {
        logDebug(`Removing event listener ${eventType} for #${targetId}.`);
        const listener = Listeners[targetId + eventType];
        return DomEventManager.removeEventHandler(targetId, eventType, listener);
    }

    /**
     * Add an event listener to the specified HTMLElement.
     * @param target The target element or identifier.
     * @param eventType The event type.
     * @param listener The event listenerj or event listener object.
     */
    static addEventHandler(target: HTMLElement | string, eventType: string, listener: EventListenerOrEventListenerObject) {
        let targetElement: HTMLElement;

        if (_isString(target)) {
            targetElement = document.getElementById(target as string);
        } else {
            targetElement = target as HTMLElement;
        }

        if (!targetElement) {
            logError(null, "Target is not a valid object reference.");
            return false;
        }

        let success: boolean;

        if (targetElement.addEventListener) {
            targetElement.addEventListener(eventType, listener, false);
            success = true;
        } else if (targetElement['attachEvent'] && typeof targetElement['attachEvent'] === 'function') {
            (targetElement as any).attachEvent('on' + eventType, listener);
            success = true;
        } else {
            logDebug(`Could not add '${eventType}' event listener for #${targetElement}`);
        }

        if (success) {
            addListener(targetElement.id, eventType, listener);
        }

        return success;
    }

    /**
     * Remove the event listener in the identifier target element's event listener list.
     * @param targetId The identifier of the element for which to remove the event listener.
     * @param eventType The type of event to remove.
     * @param listener The event listener or event listener object.
     */
    static removeEventHandler(targetId: string, eventType: string, listener?: EventListenerOrEventListenerObject) {
        let success: boolean;
        const targetElement = document.getElementById(targetId);

        if (targetElement) {
            if (targetElement.removeEventListener) {
                targetElement.removeEventListener(eventType, listener, false);
                success = true;
            } else if (targetElement['detachEvent'] && typeof targetElement['detachEvent'] === 'function') {
                (targetElement as any).detachEvent('on' + eventType, listener);
                success = true;
            }
        }

        if (success) {
            removeListener(targetId, eventType);
        } else {
            logDebug(`Could not remove '${eventType}' event listener for #${targetElement}`);
        }

        return success;
    }

    /**
     * Add a 'keydown' or 'keypress' event listener to the element identified 
     * by 'targetId', and optionally allows or blocks certain types of keystrokes.
     * @param options An object that with configuration settings.
     */
    static filterKeys(options: FilterKeysOptions) {
        const { targetId, eventType, callback, filter } = options;

        if (!_isString(targetId)) {
            logError(null, "The targetId property must be a non-blank string.");
            return false;
        }

        // Keyboard events occur in the following order: keydown->keypress->input->keyup
        // However, only the first two can be cancelled,.
        if (["keydown", "keypress", "keyup"].indexOf(eventType) === -1) {
            logError(null, "Only the 'keydown', 'keypress', and 'keyup' events are supported.");
            return false;
        }

        const target = document.getElementById(targetId);

        if (target) {
            const isKeyupEvent = eventType === "keyup";
            const { allowKeyType = FILTER_KEY_TYPES.none, blockKeyCodes, noCallbackOnPassThrough } = filter || {};
            const blockedKeyCodes = (blockKeyCodes || []).map(n => n | 0);
            const hasCallback = _isString(callback);

            /**
             * Intercepts the 'keydown' and 'keypress' events.
             * @param e The event object.
             */
            const listener = (e: KeyboardEvent) => {
                const charCode = e.which ? e.which : e.keyCode;

                // The 'keyup' event cannot be prevented, so we don't check it.
                if (!isKeyupEvent) {
                    if (allowKeyType !== FILTER_KEY_TYPES.none) {
                        if ((allowKeyType === FILTER_KEY_TYPES.alpha && !isAlpha()) ||
                            (allowKeyType === FILTER_KEY_TYPES.alphaNumeric && !isAlphaNumeric()) ||
                            (allowKeyType === FILTER_KEY_TYPES.digits && !isDigit()))
                            return cancelEvent();
                    }

                    if (blockedKeyCodes.indexOf(charCode) > -1)
                        return cancelEvent();
                }

                if (hasCallback && !noCallbackOnPassThrough) {
                    const t: any = e.target;
                    const arg = populateDictionary({
                        targetId,
                        eventType: e.type,
                        value: t.value === undefined ? "" : t.value
                    }, e);
                    dotnet && dotnet.invokeMethodAsync(AssemblyName, callback, arg);
                }

                return true;

                function isAlpha() {
                    // 95 = _
                    return charCode === 95 || /^[A-Za-z]+$/.test(e.key);
                }

                function isAlphaNumeric() {
                    return /^[\w]+$/.test(e.key);
                }

                function isDigit() {
                    return charCode > 47 && charCode < 58;
                }

                function cancelEvent() {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                }
            };

            DomEventManager.addEventHandler(target, eventType, listener);

            return true;
        } else {
            notFound();
            return false;
        }
    }

    /**
     * Return an object with all mouse-related event properties.
     * @param e The event object.
     */
    static getMouseEventArgs(e: MouseEvent) {
        return {
            altKey: e.altKey,
            button: e.button,
            buttons: e.buttons,
            clientX: e.clientX,
            clientY: e.clientY,
            ctrlKey: e.ctrlKey,
            metaKey: e.metaKey,
            movementX: e.movementX,
            movementY: e.movementY,
            offsetX: e.offsetX,
            offsetY: e.offsetY,
            pageX: e.pageX,
            pageY: e.pageY,
            screenX: e.screenX,
            screenY: e.screenY,
            shiftKey: e.shiftKey,
            which: e.which,
            x: e.x,
            y: e.y
        }
    }
}

const { addEventHandler, addEventListener, filterKeys, removeEventHandler, removeEventListener, getMouseEventArgs } = DomEventManager;

export {
    filterKeys,
    addEventHandler,
    addEventListener,
    removeEventHandler,
    removeEventListener,
    getMouseEventArgs
}
