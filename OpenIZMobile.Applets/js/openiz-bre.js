﻿/// <reference path="openiz.js"/>
/**
 * @summary Execution environment for BRE in Javascript
 * @class
 * @static
 * @description The BRE normally runs in the server as a series of rules, however bcause some rules may
 * need to be run as local rules for the user interface, this class simulates the BRE back-end service
 */
var OpenIZBre = OpenIZBre || {
    /**
     * @summary Issue priority
     * @enum
     */
    IssuePriority : {
        Error : 1,
        Warning : 2,
        Information : 3
    },
    /** 
     * @summary Represents a detected issue
     * @constructor
     * @param {string} text The textual content of the issue
     * @param {OpenIZBre.IssuePriority} priority
     * @memberof OpenIZBre
     */
    DetectedIssue: function(text, priority) {
        this.text = text;
        this.priority = priority;
    },
    /**
     * @summary Allows rules to know they're running in the UI rather than the back-end
     * @memberof OpenIZBre
     */
    $inFrontEnd: true,
    /**
     * @summary Reference sets loaded
     */
    $refSets : {},
    /** 
     * @summary Reference set base URLs
     */
    $refSetBase : [],
    /**
     * @summary The current list of triggers registered for javascript
     * @memberof OpenIZBre
     */
    _triggers: [],
    /**
     * @summary The current list of validators registered for javascript
     * @memberof OpenIZBre
     */
    _validators: [],
    /**
     * @method
     * @memberof OpenIZBre
     * @summary Simulates the simplify 
     * @deprecated
     */
    ExpandObject: function (object) { return object; },
    /**
     * @method
     * @memberof OpenIZBre
     * @summary Simulates the expand object method
     * @deprecated
     */
    SimplifyObject: function (object) { return object; },
    /**
     * @method
     * @memberof OpenIZBre
     * @summary Adds a trigger event to the triggers collection
     * @param {string} type The type of object being registered
     * @param {string} trigger The name of the BRE trigger to subscribe to
     * @param {function} callback The callback function
     */
    AddBusinessRule: function (type, trigger, callback) {
        this._triggers.push({
            type: type,
            trigger: trigger,
            callback: callback
        });
    },
    /**
     * @method
     * @memberof OpenIZBre
     * @summary Adds a validator to the validatos collection
     * @param {string} type The type of object being registered
     * @param {function} callback The callback function
     */
    AddValidator: function (type, callback) {
        this._validators.push({
            type: type,
            callback: callback
        });
    },
    /** 
     * @method
     * @memberof OpenIZBre
     * @summary Simulates the rule being executed
     */
    ExecuteRule: function (trigger, instance) {
        // Execute the rule
        var retVal = instance;
        for (var t in this._triggers)
            if (this._triggers[t].type == instance.$type) {
                var triggerResult = this._triggers[t].callback(retVal);
                retVal = triggerResult[Object.keys(triggerResult)[0]] || retVal;
            }
        return retVal;
    },
    /** 
     * @method
     * @memberof OpenIZBre
     * @summary Simulates the validation method being run
     */
    Validate: function (instance) {

        // Execute the rule
        var retVal = [];
        for (var t in this._validators)
            if (this._validators[t].type == instance.$type) {
                var issues = this._validators[t].callback(instance);
                for (var i in issues)
                    retVal.push(issues[i]);
            }
        return retVal;

    }
};