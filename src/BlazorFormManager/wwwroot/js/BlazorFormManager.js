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

    insertScripts(scripts).then(function () {
        const href = getElementByTag("base", { href: '/' }).href;
        System.import(href + "_content/BlazorFormManager/dist/bundle.js");
    });

    /**
     * Insert one or more scripts into the DOM.
     * @param {string|Array<string>} srcs A URL or array of URL of the scripts to load.
     */
    function insertScripts(srcs) {
        return new Promise(function (resolve, reject) {
            if (!(srcs instanceof Array)) srcs = [srcs];
            var count = srcs.length;
            for (var i = 0; i < srcs.length; i++) {
                var s = d.createElement("script");
                s.onload = function () { if (--count === 0) resolve(); };
                s.onerror = function () { reject(new Error("Error while loading systemjs script!")); };
                s.src = srcs[i];
                s.async = false;
                head.appendChild(s);
            }
        })
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
})(window, document);
