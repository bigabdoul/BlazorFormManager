/*
 Name:          BlazorFormManager Module Loader
 Version:       1.0.0
 Author:        Abdourahamane Kaba
 Description:   Safely inserts the systemjs module loaders, and then initializes 
                the BlazorFormManager modules and their dependencies.
 License:       Licensed under the Apache License, Version 2.0. 
                See License.txt in the project root for license information.
 Copyright:     ©2022 Numeric Horizon LLC. All rights reserved.
 */
(function (global, d) {
    "use strict";

    var head = getElementByTag("head", function (name) {
        const h = d.createElement(name);
        return d.insertBefore(h, d.body);
    });

    // insert and wait until these 2 scripts are loaded
    var scripts = [
        "https://unpkg.com/systemjs@6.11.0/dist/s.min.js",
        "https://unpkg.com/systemjs@6.11.0/dist/extras/named-register.min.js"
    ];

    var scriptCount = scripts.length;
    insertScripts(scripts);

    /**
     * Insert one or more scripts into the DOM.
     * @param {string|Array<string>} srcs A URL or array of URL of the scripts to load.
     */
    function insertScripts(srcs) {
        if (!(srcs instanceof Array)) srcs = [srcs];
        for (var i = 0; i < srcs.length; i++) {
            var s = d.createElement("script");
            s.onload = scriptLoaded;
            s.src = srcs[i];
            s.async = true;
            s.defer = true;
            head.appendChild(s);
        }
    }

    /** script onload event handler; fires after each script has been loaded. */
    function scriptLoaded() {
        // decrement the script count
        if (--scriptCount === 0) {
            // now that the 2 systemjs scripts have been loaded, insert the bundle
            const href = getElementByTag("base", { href: '/' }).href;
            insertScripts(href + "_content/BlazorFormManager/dist/bundle.js");

        } else if (scriptCount === -1) {
            // this signals that the bundle.js has been loaded as well;
            // it's now safe to assume that all modules and their 
            // dependencies are available
            registerIndex();
        }
    }

    /**
     * Get the first element identified by name, or create a new one using factory.
     * @param {string} name The tag name of the element to retrieve.
     * @param {any} factory A fallback factory function or object.
     */
    function getElementByTag(name, factory) {
        const elms = d.getElementsByTagName(name);
        return elms && elms.length && elms[0] || (typeof factory === 'function' ? factory(name) : factory);
    }

    /** 
     *  Register the 'BlazorFormManagerIndex' module, which initializes 
     *  the default instance of the BlazorFormManager class.
     */
    function registerIndex() {
        System.register("BlazorFormManagerIndex", ["BlazorFormManager", "QuillEditor"], function () {
            var BlazorFormManagerModule, QuillEditorModule;
            return {
                setters: [
                    function (blazorFormManagerModule) {
                        BlazorFormManagerModule = blazorFormManagerModule;
                    },
                    function (quillEditorModule) {
                        QuillEditorModule = quillEditorModule;
                    }
                ],
                execute: function () {
                    // make BlazorFormManager object globally available;
                    // only one instance is used for all forms' management
                    var BlazorFormManager = new BlazorFormManagerModule.BlazorFormManager();

                    Object.defineProperty(global, 'BlazorFormManager', { value: BlazorFormManager });

                    // class QuillEditor
                    var QuillEditor = QuillEditorModule.QuillEditor;

                    // define immutable properties
                    Object.defineProperty(BlazorFormManager, 'QuillEditor', { value: QuillEditor });
                    Object.defineProperty(BlazorFormManager, 'Quill', { value: {} });
                    Object.defineProperty(BlazorFormManager.Quill, 'create', {
                        /**
                        * Create a new QuillEditor instance.
                        * @param {string} selector A DOM query selector.
                        * @param {any} options The options.
                        */
                        value: function (selector, options) {
                            var editor = new QuillEditor(selector, options);
                            return editor;
                        }
                    });
                }
            };
        });

        // import the module, which will invoke the setters and execute
        System.import("BlazorFormManagerIndex");
    }
})(window, document);
