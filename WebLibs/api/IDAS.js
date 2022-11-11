import { RESTClient } from './RESTClient';

let appToken = localStorage.getItem('IDAS_AppToken') || '66B70E0B-F7C4-4829-B12A-18AD309E3970';
let authToken = localStorage.getItem('IDAS_AuthToken');
let apiBaseUrl = localStorage.getItem('IDAS_ApiBaseUrl') || 'https://api.dev.idas-cloudservices.net/api/';
let authJwtCallbackPath = localStorage.getItem('IDAS_AuthJwtCallbackPath') || '';
let authJwtToken;

export let IDASFactory = {
    async create() {
        return new Promise((resolve, reject) => {
            let promise = this.authorize()
                .then(() => {
                    let idas = new IDAS();
                    return idas.authenticateWithJwt(authJwtCallbackPath)
                });
            resolve(promise);
        });
    },

    async authorize() {
        var urlParams = new URLSearchParams(location.search);
        if (urlParams.has("m")) {
            localStorage.setItem("IDAS_MandantGuid", urlParams.get("m"));
        }
        if (urlParams.has("a")) {
            localStorage.setItem("IDAS_ApiBaseUrl", urlParams.get("a"));
        }
        if (urlParams.has('j')) { // it is JWT
            let idas = new IDAS();
            idas.authorizeWithJwt(urlParams.get('j'));
            window.location.search = "";
            return Promise.reject("redirect is required");
        }
        if (urlParams.has('t')) { // it is authToken
            localStorage.setItem('IDAS_AuthJwtRefreshToken', urlParams.get("t"));
            var refreshClient = new RESTClient(apiBaseUrl, '');
            await refreshClient.refreshToken()
                .then(() => {
                    window.location.search = "";
                });
            return Promise.reject("redirect is required");
        }
    }
}

class IDAS {
    restClient = undefined;

    authorizeWithJwt(jwtToken, mandant = '') {
        authJwtToken = jwtToken;
        mandant && localStorage.setItem('IDAS_MandantGuid', mandant);
        this.restClient = new RESTClient(apiBaseUrl, jwtToken, true);
    }

    async authenticateWithJwt(authPath) {
        return new Promise(async (resolve, reject) => {
            // no valid JWT, but try to use "refresh token" first to retrive new JWT
            var refreshClient = new RESTClient(apiBaseUrl, '');
            await refreshClient.checkRefreshToken(authJwtToken, () => {
                authJwtToken = undefined;
                // ... so repeat authenticate (should lead to /Session login page)
                new IDAS().authenticateWithJwt(authPath);
            });

            // still not valid JWT -> authenticate
            if (!refreshClient.token) {
                localStorage.setItem('IDAS_AuthJwtCallbackPath', authPath || '');
                const authEndpoint = (new URL(window.location.href).origin) + authPath;
                let authUrlCallback = `${authEndpoint}?r=%target%&j=%jwt%&m=%mandant%`;
                authUrlCallback = authUrlCallback.replace('%target%', encodeURIComponent(window.location.href));
    
                const url = new URL(apiBaseUrl);
                url.pathname = "/Session";
                url.search = `?a=${appToken}&r=${encodeURIComponent(authUrlCallback)}`;
                let jwtUrl = url.toString();
    
                window.location = jwtUrl;
                reject('not authenticated yet');
            } else {
                this.authorizeWithJwt(refreshClient.token);
                resolve(this);
            }    
        });
    }

    mandantGuid = localStorage.getItem('IDAS_MandantGuid');

    auth = {
        _self: this,
        async getCurrentAuthToken() {
            return await this._self.restClient.put('/Login/Update/', { Token: authToken })
        },
    };

    mandanten = {
        _self: this,
        async getAll() {
            return await this._self.restClient.get('/Mandanten');
        },
        async get(guid) {
            return await this._self.restClient.get(`/Mandanten/${guid}`);
        },
        async save(m) {
            await this._self.restClient.put('/Mandanten', m);
        },
    };

    benutzer = {
        _self: this,
        async getAll(mandantGuid) {
            return await this._self.restClient.get(`/BenutzerListe/${mandantGuid }/?mitRollenUndRechten=true`);
        },
        async get(guid) {
            return await this._self.restClient.get(`/Benutzer/${guid}`);
        },
        async save(m) {
            await this._self.restClient.put('/Benutzer', m);
        },
    };

    feedback = {
        _self: this,
        async getAll() {
            return await this._self.restClient.get('/Feedback/');
        },
        async get(guid) {
            return await this._self.restClient.get(`/Feedback/${guid}`);
        },
        async save(m) {
            await this._self.restClient.put('/Feedback', m);
        },
        async comment(guid, commentData) {
            await this._self.restClient.put(`/FeedbackKommentar/${guid}`, commentData);
        },
        async attachFile(guid, filename, data) {
            await this._self.restClient.put(`/FeedbackAttachment/?feedbackGuid=${guid}&filename=${filename}`, data);
        },
        async deleteFile(guid) {
            await this._self.restClient.delete(`/FeedbackAttachment/${guid}`);
        },
    };

    rollen = {
        _self: this,
        async getAll() {
            return await this._self.restClient.get('/Rollen');
        },
        async save(m) {
            await this._self.restClient.put('/Rollen', m);
        },
    };

    vorgaenge = {
        _self: this,
        async getByVorgangsnummer(vorgangsNummer, jahr) {
            return await this._self.restClient.get(`/Vorgang/${vorgangsNummer}/${jahr}`);
        },
    };

    positionen = {
        _self: this,
        async getByPcode(pcode) {
            return await this._self.restClient.get(`/BelegPositionen/GetByPcode/${pcode}`);
        },
        async get(guid) {
            return await this._self.restClient.get(`/BelegPositionen/Get/${guid}`);
        },
    };
}
