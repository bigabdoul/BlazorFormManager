import { Forms } from "./Shared";

const CONSOLE_FUNC = { info: "info", warn: "warn", error: "error", log: "log" };
const LOG_LEVEL = { none: 0, information: 1, warning: 2, error: 3, debug: 4 };

class ConsoleLogger {

    static logInfo(formId: string, ...args: any[]) {
        this.log(formId, CONSOLE_FUNC.info, ...args);
    }

    static logWarning(formId: string, ...args: any[]) {
        this.log(formId, CONSOLE_FUNC.warn, ...args);
    }

    static logError(formId: string, ...args: any[]) {
        this.log(formId, CONSOLE_FUNC.error, ...args);
    }

    static logDebug(formId: string, ...args: any[]) {
        this.log(formId, CONSOLE_FUNC.log, ...args);
    }

    static log(formId: string, func: string, ...args: any[]) {
        const { logLevel = LOG_LEVEL.debug } = Forms[formId] || { logLevel: LOG_LEVEL.debug };

        if (logLevel === LOG_LEVEL.none) return;

        func || (func = CONSOLE_FUNC.info);
        args || (args = []);
        args.unshift(`${func === "log" ? "debug" : func}: BlazorFormManager:`);
        
        if (logLevel === LOG_LEVEL.debug)
            console[func].apply(console, args);

        else if (func === CONSOLE_FUNC.error && logLevel >= LOG_LEVEL.error)
            console.error.apply(console, args);

        else if (func === CONSOLE_FUNC.warn && logLevel >= LOG_LEVEL.warning)
            console.warn.apply(console, args);

        else if (func === CONSOLE_FUNC.info && logLevel >= LOG_LEVEL.information)
            console.info.apply(console, args);
    }
}

const { log, logDebug, logError, logInfo, logWarning } = ConsoleLogger;

export {
    log,
    logDebug,
    logError,
    logInfo,
    logWarning,
    ConsoleLogger
}