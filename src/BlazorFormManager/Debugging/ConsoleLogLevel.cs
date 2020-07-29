// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BlazorFormManager.Debugging
{
    /// <summary>
    /// JavaScript console log level enumerations.
    /// </summary>
    public enum ConsoleLogLevel
    {
        /// <summary>
        /// Log nothing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Log information only.
        /// </summary>
        Information = 1,

        /// <summary>
        /// Log warnings only.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Log errors only.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Log everything.
        /// </summary>
        Debug = 4,
    }
}
