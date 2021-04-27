(function(f){if(typeof exports==="object"&&typeof module!=="undefined"){module.exports=f()}else if(typeof define==="function"&&define.amd){define([],f)}else{var g;if(typeof window!=="undefined"){g=window}else if(typeof global!=="undefined"){g=global}else if(typeof self!=="undefined"){g=self}else{g=this}g.signalR = f()}})(function(){var define,module,exports;return (function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class Base64EncodedHubProtocol {
    constructor(protocol) {
        this.wrappedProtocol = protocol;
        this.name = this.wrappedProtocol.name;
        this.type = 1 /* Text */;
    }
    parseMessages(input) {
        // The format of the message is `size:message;`
        let pos = input.indexOf(":");
        if (pos == -1 || !input.endsWith(";")) {
            throw new Error("Invalid payload.");
        }
        let lenStr = input.substring(0, pos);
        if (!/^[0-9]+$/.test(lenStr)) {
            throw new Error(`Invalid length: '${lenStr}'`);
        }
        let messageSize = parseInt(lenStr, 10);
        // 2 accounts for ':' after message size and trailing ';'
        if (messageSize != input.length - pos - 2) {
            throw new Error("Invalid message size.");
        }
        let encodedMessage = input.substring(pos + 1, input.length - 1);
        // atob/btoa are browsers APIs but they can be polyfilled. If this becomes problematic we can use
        // base64-js module
        let s = atob(encodedMessage);
        let payload = new Uint8Array(s.length);
        for (let i = 0; i < payload.length; i++) {
            payload[i] = s.charCodeAt(i);
        }
        return this.wrappedProtocol.parseMessages(payload.buffer);
    }
    writeMessage(message) {
        let payload = new Uint8Array(this.wrappedProtocol.writeMessage(message));
        let s = "";
        for (let i = 0; i < payload.byteLength; i++) {
            s += String.fromCharCode(payload[i]);
        }
        // atob/btoa are browsers APIs but they can be polyfilled. If this becomes problematic we can use
        // base64-js module
        let encodedMessage = btoa(s);
        return `${encodedMessage.length.toString()}:${encodedMessage};`;
    }
}
exports.Base64EncodedHubProtocol = Base64EncodedHubProtocol;

},{}],2:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var TextMessageFormat;
(function (TextMessageFormat) {
    const RecordSeparator = String.fromCharCode(0x1e);
    function write(output) {
        return `${output}${RecordSeparator}`;
    }
    TextMessageFormat.write = write;
    function parse(input) {
        if (!input.endsWith(RecordSeparator)) {
            throw new Error("Message is incomplete.");
        }
        let messages = input.split(RecordSeparator);
        messages.pop();
        return messages;
    }
    TextMessageFormat.parse = parse;
})(TextMessageFormat = exports.TextMessageFormat || (exports.TextMessageFormat = {}));
var BinaryMessageFormat;
(function (BinaryMessageFormat) {
    function write(output) {
        let size = output.byteLength;
        let buffer = new Uint8Array(size + 8);
        // javascript bitwise operators only support 32-bit integers
        for (let i = 7; i >= 4; i--) {
            buffer[i] = size & 0xff;
            size = size >> 8;
        }
        buffer.set(output, 8);
        return buffer.buffer;
    }
    BinaryMessageFormat.write = write;
    function parse(input) {
        let result = [];
        let uint8Array = new Uint8Array(input);
        // 8 - the length prefix size
        for (let offset = 0; offset < input.byteLength;) {
            if (input.byteLength < offset + 8) {
                throw new Error("Cannot read message size");
            }
            // Note javascript bitwise operators only support 32-bit integers - for now cutting bigger messages.
            // Tracking bug https://github.com/aspnet/SignalR/issues/613
            if (!(uint8Array[offset] == 0 && uint8Array[offset + 1] == 0 && uint8Array[offset + 2] == 0
                && uint8Array[offset + 3] == 0 && (uint8Array[offset + 4] & 0x80) == 0)) {
                throw new Error("Messages bigger than 2147483647 bytes are not supported");
            }
            let size = 0;
            for (let i = 4; i < 8; i++) {
                size = (size << 8) | uint8Array[offset + i];
            }
            if (uint8Array.byteLength >= (offset + 8 + size)) {
                result.push(uint8Array.slice(offset + 8, offset + 8 + size));
            }
            else {
                throw new Error("Incomplete message");
            }
            offset = offset + 8 + size;
        }
        return result;
    }
    BinaryMessageFormat.parse = parse;
})(BinaryMessageFormat = exports.BinaryMessageFormat || (exports.BinaryMessageFormat = {}));

},{}],3:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const HttpError_1 = require("./HttpError");
class HttpClient {
    get(url, headers) {
        return this.xhr("GET", url, headers);
    }
    options(url, headers) {
        return this.xhr("OPTIONS", url, headers);
    }
    post(url, content, headers) {
        return this.xhr("POST", url, headers, content);
    }
    xhr(method, url, headers, content) {
        return new Promise((resolve, reject) => {
            let xhr = new XMLHttpRequest();
            xhr.open(method, url, true);
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            if (headers) {
                headers.forEach((value, header) => xhr.setRequestHeader(header, value));
            }
            xhr.send(content);
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.response);
                }
                else {
                    reject(new HttpError_1.HttpError(xhr.statusText, xhr.status));
                }
            };
            xhr.onerror = () => {
                reject(new HttpError_1.HttpError(xhr.statusText, xhr.status));
            };
        });
    }
}
exports.HttpClient = HttpClient;

},{"./HttpError":5}],4:[function(require,module,exports){
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const Transports_1 = require("./Transports");
const HttpClient_1 = require("./HttpClient");
class HttpConnection {
    constructor(url, options = {}) {
        this.features = {};
        this.url = url;
        this.httpClient = options.httpClient || new HttpClient_1.HttpClient();
        this.connectionState = 0 /* Initial */;
        this.options = options;
    }
    start() {
        return __awaiter(this, void 0, void 0, function* () {
            if (this.connectionState != 0 /* Initial */) {
                return Promise.reject(new Error("Cannot start a connection that is not in the 'Initial' state."));
            }
            this.connectionState = 1 /* Connecting */;
            this.startPromise = this.startInternal();
            return this.startPromise;
        });
    }
    startInternal() {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                let negotiatePayload = yield this.httpClient.options(this.url);
                let negotiateResponse = JSON.parse(negotiatePayload);
                this.connectionId = negotiateResponse.connectionId;
                // the user tries to stop the the connection when it is being started
                if (this.connectionState == 3 /* Disconnected */) {
                    return;
                }
                this.url += (this.url.indexOf("?") == -1 ? "?" : "&") + `id=${this.connectionId}`;
                this.transport = this.createTransport(this.options.transport, negotiateResponse.availableTransports);
                this.transport.onDataReceived = this.onDataReceived;
                this.transport.onClosed = e => this.stopConnection(true, e);
                let requestedTransferMode = this.features.transferMode === 2 /* Binary */
                    ? 2 /* Binary */
                    : 1 /* Text */;
                this.features.transferMode = yield this.transport.connect(this.url, requestedTransferMode);
                // only change the state if we were connecting to not overwrite
                // the state if the connection is already marked as Disconnected
                this.changeState(1 /* Connecting */, 2 /* Connected */);
            }
            catch (e) {
                console.log("Failed to start the connection. " + e);
                this.connectionState = 3 /* Disconnected */;
                this.transport = null;
                throw e;
            }
            ;
        });
    }
    createTransport(transport, availableTransports) {
        if (!transport && availableTransports.length > 0) {
            transport = Transports_1.TransportType[availableTransports[0]];
        }
        if (transport === Transports_1.TransportType.WebSockets && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.WebSocketTransport();
        }
        if (transport === Transports_1.TransportType.ServerSentEvents && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.ServerSentEventsTransport(this.httpClient);
        }
        if (transport === Transports_1.TransportType.LongPolling && availableTransports.indexOf(Transports_1.TransportType[transport]) >= 0) {
            return new Transports_1.LongPollingTransport(this.httpClient);
        }
        if (this.isITransport(transport)) {
            return transport;
        }
        throw new Error("No available transports found.");
    }
    isITransport(transport) {
        return typeof (transport) === "object" && "connect" in transport;
    }
    changeState(from, to) {
        if (this.connectionState == from) {
            this.connectionState = to;
            return true;
        }
        return false;
    }
    send(data) {
        if (this.connectionState != 2 /* Connected */) {
            throw new Error("Cannot send data if the connection is not in the 'Connected' State");
        }
        return this.transport.send(data);
    }
    stop() {
        return __awaiter(this, void 0, void 0, function* () {
            let previousState = this.connectionState;
            this.connectionState = 3 /* Disconnected */;
            try {
                yield this.startPromise;
            }
            catch (e) {
                // this exception is returned to the user as a rejected Promise from the start method
            }
            this.stopConnection(/*raiseClosed*/ previousState == 2 /* Connected */);
        });
    }
    stopConnection(raiseClosed, error) {
        if (this.transport) {
            this.transport.stop();
            this.transport = null;
        }
        this.connectionState = 3 /* Disconnected */;
        if (raiseClosed && this.onClosed) {
            this.onClosed(error);
        }
    }
}
exports.HttpConnection = HttpConnection;

},{"./HttpClient":3,"./Transports":9}],5:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class HttpError extends Error {
    constructor(errorMessage, statusCode) {
        super(errorMessage);
        this.statusCode = statusCode;
    }
}
exports.HttpError = HttpError;

},{}],6:[function(require,module,exports){
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const Observable_1 = require("./Observable");
const JsonHubProtocol_1 = require("./JsonHubProtocol");
const Formatters_1 = require("./Formatters");
const Base64EncodedHubProtocol_1 = require("./Base64EncodedHubProtocol");
var Transports_1 = require("./Transports");
exports.TransportType = Transports_1.TransportType;
var HttpConnection_1 = require("./HttpConnection");
exports.HttpConnection = HttpConnection_1.HttpConnection;
var JsonHubProtocol_2 = require("./JsonHubProtocol");
exports.JsonHubProtocol = JsonHubProtocol_2.JsonHubProtocol;
class HubConnection {
    constructor(connection, protocol = new JsonHubProtocol_1.JsonHubProtocol()) {
        this.connection = connection;
        this.protocol = protocol || new JsonHubProtocol_1.JsonHubProtocol();
        this.connection.onDataReceived = data => {
            this.onDataReceived(data);
        };
        this.connection.onClosed = (error) => {
            this.onConnectionClosed(error);
        };
        this.callbacks = new Map();
        this.methods = new Map();
        this.id = 0;
    }
    onDataReceived(data) {
        // Parse the messages
        let messages = this.protocol.parseMessages(data);
        for (var i = 0; i < messages.length; ++i) {
            var message = messages[i];
            switch (message.type) {
                case 1 /* Invocation */:
                    this.invokeClientMethod(message);
                    break;
                case 2 /* Result */:
                case 3 /* Completion */:
                    let callback = this.callbacks.get(message.invocationId);
                    if (callback != null) {
                        callback(message);
                        if (message.type == 3 /* Completion */) {
                            this.callbacks.delete(message.invocationId);
                        }
                    }
                    break;
                default:
                    console.log("Invalid message type: " + data);
                    break;
            }
        }
    }
    invokeClientMethod(invocationMessage) {
        let method = this.methods.get(invocationMessage.target);
        if (method) {
            method.apply(this, invocationMessage.arguments);
            if (!invocationMessage.nonblocking) {
                // TODO: send result back to the server?
            }
        }
        else {
            console.log(`No client method with the name '${invocationMessage.target}' found.`);
        }
    }
    onConnectionClosed(error) {
        let errorCompletionMessage = {
            type: 3 /* Completion */,
            invocationId: "-1",
            error: error ? error.message : "Invocation cancelled due to connection being closed.",
        };
        this.callbacks.forEach(callback => {
            callback(errorCompletionMessage);
        });
        this.callbacks.clear();
        if (this.connectionClosedCallback) {
            this.connectionClosedCallback(error);
        }
    }
    start() {
        return __awaiter(this, void 0, void 0, function* () {
            let requestedTransferMode = (this.protocol.type === 2 /* Binary */)
                ? 2 /* Binary */
                : 1 /* Text */;
            this.connection.features.transferMode = requestedTransferMode;
            yield this.connection.start();
            var actualTransferMode = this.connection.features.transferMode;
            yield this.connection.send(Formatters_1.TextMessageFormat.write(JSON.stringify({ protocol: this.protocol.name })));
            if (requestedTransferMode === 2 /* Binary */ && actualTransferMode === 1 /* Text */) {
                this.protocol = new Base64EncodedHubProtocol_1.Base64EncodedHubProtocol(this.protocol);
            }
        });
    }
    stop() {
        return this.connection.stop();
    }
    stream(methodName, ...args) {
        let invocationDescriptor = this.createInvocation(methodName, args, false);
        let subject = new Observable_1.Subject();
        this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent) => {
            if (invocationEvent.type === 3 /* Completion */) {
                let completionMessage = invocationEvent;
                if (completionMessage.error) {
                    subject.error(new Error(completionMessage.error));
                }
                else if (completionMessage.result) {
                    subject.error(new Error("Server provided a result in a completion response to a streamed invocation."));
                }
                else {
                    // TODO: Log a warning if there's a payload?
                    subject.complete();
                }
            }
            else {
                subject.next(invocationEvent.item);
            }
        });
        let message = this.protocol.writeMessage(invocationDescriptor);
        this.connection.send(message)
            .catch(e => {
            subject.error(e);
            this.callbacks.delete(invocationDescriptor.invocationId);
        });
        return subject;
    }
    send(methodName, ...args) {
        let invocationDescriptor = this.createInvocation(methodName, args, true);
        let message = this.protocol.writeMessage(invocationDescriptor);
        return this.connection.send(message);
    }
    invoke(methodName, ...args) {
        let invocationDescriptor = this.createInvocation(methodName, args, false);
        let p = new Promise((resolve, reject) => {
            this.callbacks.set(invocationDescriptor.invocationId, (invocationEvent) => {
                if (invocationEvent.type === 3 /* Completion */) {
                    let completionMessage = invocationEvent;
                    if (completionMessage.error) {
                        reject(new Error(completionMessage.error));
                    }
                    else {
                        resolve(completionMessage.result);
                    }
                }
                else {
                    reject(new Error("Streaming methods must be invoked using HubConnection.stream"));
                }
            });
            let message = this.protocol.writeMessage(invocationDescriptor);
            this.connection.send(message)
                .catch(e => {
                reject(e);
                this.callbacks.delete(invocationDescriptor.invocationId);
            });
        });
        return p;
    }
    on(methodName, method) {
        this.methods.set(methodName, method);
    }
    set onClosed(callback) {
        this.connectionClosedCallback = callback;
    }
    createInvocation(methodName, args, nonblocking) {
        let id = this.id;
        this.id++;
        return {
            type: 1 /* Invocation */,
            invocationId: id.toString(),
            target: methodName,
            arguments: args,
            nonblocking: nonblocking
        };
    }
}
exports.HubConnection = HubConnection;

},{"./Base64EncodedHubProtocol":1,"./Formatters":2,"./HttpConnection":4,"./JsonHubProtocol":7,"./Observable":8,"./Transports":9}],7:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Formatters_1 = require("./Formatters");
class JsonHubProtocol {
    constructor() {
        this.name = "json";
        this.type = 1 /* Text */;
    }
    parseMessages(input) {
        if (!input) {
            return [];
        }
        // Parse the messages
        let messages = Formatters_1.TextMessageFormat.parse(input);
        let hubMessages = [];
        for (var i = 0; i < messages.length; ++i) {
            hubMessages.push(JSON.parse(messages[i]));
        }
        return hubMessages;
    }
    writeMessage(message) {
        return Formatters_1.TextMessageFormat.write(JSON.stringify(message));
    }
}
exports.JsonHubProtocol = JsonHubProtocol;

},{"./Formatters":2}],8:[function(require,module,exports){
"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class Subject {
    constructor() {
        this.observers = [];
    }
    next(item) {
        for (let observer of this.observers) {
            observer.next(item);
        }
    }
    error(err) {
        for (let observer of this.observers) {
            observer.error(err);
        }
    }
    complete() {
        for (let observer of this.observers) {
            observer.complete();
        }
    }
    subscribe(observer) {
        this.observers.push(observer);
    }
}
exports.Subject = Subject;

},{}],9:[function(require,module,exports){
"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const HttpError_1 = require("./HttpError");
var TransportType;
(function (TransportType) {
    TransportType[TransportType["WebSockets"] = 0] = "WebSockets";
    TransportType[TransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    TransportType[TransportType["LongPolling"] = 2] = "LongPolling";
})(TransportType = exports.TransportType || (exports.TransportType = {}));
class WebSocketTransport {
    connect(url, requestedTransferMode) {
        return new Promise((resolve, reject) => {
            url = url.replace(/^http/, "ws");
            let webSocket = new WebSocket(url);
            if (requestedTransferMode == 2 /* Binary */) {
                webSocket.binaryType = "arraybuffer";
            }
            webSocket.onopen = (event) => {
                console.log(`WebSocket connected to ${url}`);
                this.webSocket = webSocket;
                resolve(requestedTransferMode);
            };
            webSocket.onerror = (event) => {
                reject();
            };
            webSocket.onmessage = (message) => {
                console.log(`(WebSockets transport) data received: ${message.data}`);
                if (this.onDataReceived) {
                    this.onDataReceived(message.data);
                }
            };
            webSocket.onclose = (event) => {
                // webSocket will be null if the transport did not start successfully
                if (this.onClosed && this.webSocket) {
                    if (event.wasClean === false || event.code !== 1000) {
                        this.onClosed(new Error(`Websocket closed with status code: ${event.code} (${event.reason})`));
                    }
                    else {
                        this.onClosed();
                    }
                }
            };
        });
    }
    send(data) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.webSocket.send(data);
            return Promise.resolve();
        }
        return Promise.reject("WebSocket is not in the OPEN state");
    }
    stop() {
        if (this.webSocket) {
            this.webSocket.close();
            this.webSocket = null;
        }
    }
}
exports.WebSocketTransport = WebSocketTransport;
class ServerSentEventsTransport {
    constructor(httpClient) {
        this.httpClient = httpClient;
    }
    connect(url, requestedTransferMode) {
        if (typeof (EventSource) === "undefined") {
            Promise.reject("EventSource not supported by the browser.");
        }
        this.url = url;
        return new Promise((resolve, reject) => {
            let eventSource = new EventSource(this.url);
            try {
                eventSource.onmessage = (e) => {
                    if (this.onDataReceived) {
                        try {
                            console.log(`(SSE transport) data received: ${e.data}`);
                            this.onDataReceived(e.data);
                        }
                        catch (error) {
                            if (this.onClosed) {
                                this.onClosed(error);
                            }
                            return;
                        }
                    }
                };
                eventSource.onerror = (e) => {
                    reject();
                    // don't report an error if the transport did not start successfully
                    if (this.eventSource && this.onClosed) {
                        this.onClosed(new Error(e.message || "Error occurred"));
                    }
                };
                eventSource.onopen = () => {
                    console.log(`SSE connected to ${this.url}`);
                    this.eventSource = eventSource;
                    // SSE is a text protocol
                    resolve(1 /* Text */);
                };
            }
            catch (e) {
                return Promise.reject(e);
            }
        });
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.httpClient, this.url, data);
        });
    }
    stop() {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
    }
}
exports.ServerSentEventsTransport = ServerSentEventsTransport;
class LongPollingTransport {
    constructor(httpClient) {
        this.httpClient = httpClient;
    }
    connect(url, requestedTransferMode) {
        this.url = url;
        this.shouldPoll = true;
        this.poll(this.url, requestedTransferMode);
        return Promise.resolve(requestedTransferMode);
    }
    poll(url, transferMode) {
        if (!this.shouldPoll) {
            return;
        }
        let pollXhr = new XMLHttpRequest();
        if (transferMode === 2 /* Binary */) {
            pollXhr.responseType = "arraybuffer";
        }
        pollXhr.onload = () => {
            if (pollXhr.status == 200) {
                if (this.onDataReceived) {
                    try {
                        if (pollXhr.response) {
                            console.log(`(LongPolling transport) data received: ${pollXhr.response}`);
                            this.onDataReceived(pollXhr.response);
                        }
                        else {
                            console.log(`(LongPolling transport) timed out`);
                        }
                    }
                    catch (error) {
                        if (this.onClosed) {
                            this.onClosed(error);
                        }
                        return;
                    }
                }
                this.poll(url, transferMode);
            }
            else if (this.pollXhr.status == 204) {
                if (this.onClosed) {
                    this.onClosed();
                }
            }
            else {
                if (this.onClosed) {
                    this.onClosed(new HttpError_1.HttpError(pollXhr.statusText, pollXhr.status));
                }
            }
        };
        pollXhr.onerror = () => {
            if (this.onClosed) {
                // network related error or denied cross domain request
                this.onClosed(new Error("Sending HTTP request failed."));
            }
        };
        pollXhr.ontimeout = () => {
            this.poll(url, transferMode);
        };
        this.pollXhr = pollXhr;
        this.pollXhr.open("GET", url, true);
        // TODO: consider making timeout configurable
        this.pollXhr.timeout = 120000;
        this.pollXhr.send();
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.httpClient, this.url, data);
        });
    }
    stop() {
        this.shouldPoll = false;
        if (this.pollXhr) {
            this.pollXhr.abort();
            this.pollXhr = null;
        }
    }
}
exports.LongPollingTransport = LongPollingTransport;
const headers = new Map();
function send(httpClient, url, data) {
    return __awaiter(this, void 0, void 0, function* () {
        yield httpClient.post(url, data, headers);
    });
}

},{"./HttpError":5}]},{},[6])(6)
});