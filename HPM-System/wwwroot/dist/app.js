/******/ (() => { // webpackBootstrap
/******/ 	// The require scope
/******/ 	var __webpack_require__ = {};
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
// This entry needs to be wrapped in an IIFE because it needs to be isolated against other entry modules.
(() => {
/*!****************************!*\
  !*** ./wwwroot/js/site.js ***!
  \****************************/
function _typeof(o) { "@babel/helpers - typeof"; return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (o) { return typeof o; } : function (o) { return o && "function" == typeof Symbol && o.constructor === Symbol && o !== Symbol.prototype ? "symbol" : typeof o; }, _typeof(o); }
function _regenerator() { /*! regenerator-runtime -- Copyright (c) 2014-present, Facebook, Inc. -- license (MIT): https://github.com/babel/babel/blob/main/packages/babel-helpers/LICENSE */ var e, t, r = "function" == typeof Symbol ? Symbol : {}, n = r.iterator || "@@iterator", o = r.toStringTag || "@@toStringTag"; function i(r, n, o, i) { var c = n && n.prototype instanceof Generator ? n : Generator, u = Object.create(c.prototype); return _regeneratorDefine2(u, "_invoke", function (r, n, o) { var i, c, u, f = 0, p = o || [], y = !1, G = { p: 0, n: 0, v: e, a: d, f: d.bind(e, 4), d: function d(t, r) { return i = t, c = 0, u = e, G.n = r, a; } }; function d(r, n) { for (c = r, u = n, t = 0; !y && f && !o && t < p.length; t++) { var o, i = p[t], d = G.p, l = i[2]; r > 3 ? (o = l === n) && (u = i[(c = i[4]) ? 5 : (c = 3, 3)], i[4] = i[5] = e) : i[0] <= d && ((o = r < 2 && d < i[1]) ? (c = 0, G.v = n, G.n = i[1]) : d < l && (o = r < 3 || i[0] > n || n > l) && (i[4] = r, i[5] = n, G.n = l, c = 0)); } if (o || r > 1) return a; throw y = !0, n; } return function (o, p, l) { if (f > 1) throw TypeError("Generator is already running"); for (y && 1 === p && d(p, l), c = p, u = l; (t = c < 2 ? e : u) || !y;) { i || (c ? c < 3 ? (c > 1 && (G.n = -1), d(c, u)) : G.n = u : G.v = u); try { if (f = 2, i) { if (c || (o = "next"), t = i[o]) { if (!(t = t.call(i, u))) throw TypeError("iterator result is not an object"); if (!t.done) return t; u = t.value, c < 2 && (c = 0); } else 1 === c && (t = i["return"]) && t.call(i), c < 2 && (u = TypeError("The iterator does not provide a '" + o + "' method"), c = 1); i = e; } else if ((t = (y = G.n < 0) ? u : r.call(n, G)) !== a) break; } catch (t) { i = e, c = 1, u = t; } finally { f = 1; } } return { value: t, done: y }; }; }(r, o, i), !0), u; } var a = {}; function Generator() {} function GeneratorFunction() {} function GeneratorFunctionPrototype() {} t = Object.getPrototypeOf; var c = [][n] ? t(t([][n]())) : (_regeneratorDefine2(t = {}, n, function () { return this; }), t), u = GeneratorFunctionPrototype.prototype = Generator.prototype = Object.create(c); function f(e) { return Object.setPrototypeOf ? Object.setPrototypeOf(e, GeneratorFunctionPrototype) : (e.__proto__ = GeneratorFunctionPrototype, _regeneratorDefine2(e, o, "GeneratorFunction")), e.prototype = Object.create(u), e; } return GeneratorFunction.prototype = GeneratorFunctionPrototype, _regeneratorDefine2(u, "constructor", GeneratorFunctionPrototype), _regeneratorDefine2(GeneratorFunctionPrototype, "constructor", GeneratorFunction), GeneratorFunction.displayName = "GeneratorFunction", _regeneratorDefine2(GeneratorFunctionPrototype, o, "GeneratorFunction"), _regeneratorDefine2(u), _regeneratorDefine2(u, o, "Generator"), _regeneratorDefine2(u, n, function () { return this; }), _regeneratorDefine2(u, "toString", function () { return "[object Generator]"; }), (_regenerator = function _regenerator() { return { w: i, m: f }; })(); }
function _regeneratorDefine2(e, r, n, t) { var i = Object.defineProperty; try { i({}, "", {}); } catch (e) { i = 0; } _regeneratorDefine2 = function _regeneratorDefine(e, r, n, t) { function o(r, n) { _regeneratorDefine2(e, r, function (e) { return this._invoke(r, n, e); }); } r ? i ? i(e, r, { value: n, enumerable: !t, configurable: !t, writable: !t }) : e[r] = n : (o("next", 0), o("throw", 1), o("return", 2)); }, _regeneratorDefine2(e, r, n, t); }
function asyncGeneratorStep(n, t, e, r, o, a, c) { try { var i = n[a](c), u = i.value; } catch (n) { return void e(n); } i.done ? t(u) : Promise.resolve(u).then(r, o); }
function _asyncToGenerator(n) { return function () { var t = this, e = arguments; return new Promise(function (r, o) { var a = n.apply(t, e); function _next(n) { asyncGeneratorStep(a, r, o, _next, _throw, "next", n); } function _throw(n) { asyncGeneratorStep(a, r, o, _next, _throw, "throw", n); } _next(void 0); }); }; }
function _classCallCheck(a, n) { if (!(a instanceof n)) throw new TypeError("Cannot call a class as a function"); }
function _defineProperties(e, r) { for (var t = 0; t < r.length; t++) { var o = r[t]; o.enumerable = o.enumerable || !1, o.configurable = !0, "value" in o && (o.writable = !0), Object.defineProperty(e, _toPropertyKey(o.key), o); } }
function _createClass(e, r, t) { return r && _defineProperties(e.prototype, r), t && _defineProperties(e, t), Object.defineProperty(e, "prototype", { writable: !1 }), e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == _typeof(i) ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != _typeof(t) || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != _typeof(i)) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
var AuthManager = /*#__PURE__*/function () {
  function AuthManager() {
    _classCallCheck(this, AuthManager);
    this.tokenKey = 'hpm_auth_token';
    this.userDataKey = 'hpm_user_data';
    this.authApiUrl = '/api/auth';
    this.isAuthenticated = false;
    this.userData = null;

    // Автоматически инициализируем при загрузке
    this.initialize();
  }

  /**
   * Инициализация менеджера аутентификации
   */
  return _createClass(AuthManager, [{
    key: "initialize",
    value: (function () {
      var _initialize = _asyncToGenerator(/*#__PURE__*/_regenerator().m(function _callee() {
        var urlParams, authCode;
        return _regenerator().w(function (_context) {
          while (1) switch (_context.n) {
            case 0:
              // Проверяем наличие кода аутентификации в URL
              urlParams = new URLSearchParams(window.location.search);
              authCode = urlParams.get('auth');
              if (!authCode) {
                _context.n = 2;
                break;
              }
              console.log('Найден код аутентификации в URL');
              _context.n = 1;
              return this.exchangeAuthCode(authCode);
            case 1:
              // Удаляем код из URL после обработки
              this.clearAuthCodeFromUrl();
              _context.n = 3;
              break;
            case 2:
              _context.n = 3;
              return this.checkStoredToken();
            case 3:
              // Обновляем UI в зависимости от состояния авторизации
              this.updateUI();
            case 4:
              return _context.a(2);
          }
        }, _callee, this);
      }));
      function initialize() {
        return _initialize.apply(this, arguments);
      }
      return initialize;
    }()
    /**
     * Обменивает код аутентификации на токен
     */
    )
  }, {
    key: "exchangeAuthCode",
    value: (function () {
      var _exchangeAuthCode = _asyncToGenerator(/*#__PURE__*/_regenerator().m(function _callee2(authCode) {
        var response, result, _t;
        return _regenerator().w(function (_context2) {
          while (1) switch (_context2.p = _context2.n) {
            case 0:
              _context2.p = 0;
              _context2.n = 1;
              return fetch("".concat(this.authApiUrl, "/exchange-code"), {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                  authCode: authCode
                })
              });
            case 1:
              response = _context2.v;
              _context2.n = 2;
              return response.json();
            case 2:
              result = _context2.v;
              if (response.ok && result.isAuthenticated) {
                this.setAuthData(result.token, {
                  userId: result.userId,
                  email: result.email,
                  phoneNumber: result.phoneNumber
                });
                console.log('✅ Авторизация успешна');
                this.showNotification('Добро пожаловать!', 'success');
              } else {
                console.warn('❌ Ошибка при обмене кода аутентификации:', result.message);
                this.clearAuthData();
                this.showNotification(result.message || 'Ошибка авторизации', 'error');
              }
              _context2.n = 4;
              break;
            case 3:
              _context2.p = 3;
              _t = _context2.v;
              console.error('❌ Ошибка при обмене кода аутентификации:', _t);
              this.clearAuthData();
              this.showNotification('Произошла ошибка при авторизации', 'error');
            case 4:
              return _context2.a(2);
          }
        }, _callee2, this, [[0, 3]]);
      }));
      function exchangeAuthCode(_x) {
        return _exchangeAuthCode.apply(this, arguments);
      }
      return exchangeAuthCode;
    }()
    /**
     * Проверяет сохраненный токен
     */
    )
  }, {
    key: "checkStoredToken",
    value: (function () {
      var _checkStoredToken = _asyncToGenerator(/*#__PURE__*/_regenerator().m(function _callee3() {
        var token, response, result, _t2;
        return _regenerator().w(function (_context3) {
          while (1) switch (_context3.p = _context3.n) {
            case 0:
              token = localStorage.getItem(this.tokenKey);
              if (token) {
                _context3.n = 1;
                break;
              }
              this.clearAuthData();
              return _context3.a(2);
            case 1:
              _context3.p = 1;
              _context3.n = 2;
              return fetch("".concat(this.authApiUrl, "/validate-token"), {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                  token: token
                })
              });
            case 2:
              response = _context3.v;
              _context3.n = 3;
              return response.json();
            case 3:
              result = _context3.v;
              if (response.ok && result.isAuthenticated) {
                this.setAuthData(token, {
                  userId: result.userId,
                  email: result.email,
                  phoneNumber: result.phoneNumber
                });
                console.log('✅ Токен валиден, пользователь авторизован');
              } else {
                console.log('❌ Токен невалиден, очищаем данные');
                this.clearAuthData();
              }
              _context3.n = 5;
              break;
            case 4:
              _context3.p = 4;
              _t2 = _context3.v;
              console.error('❌ Ошибка при проверке токена:', _t2);
              this.clearAuthData();
            case 5:
              return _context3.a(2);
          }
        }, _callee3, this, [[1, 4]]);
      }));
      function checkStoredToken() {
        return _checkStoredToken.apply(this, arguments);
      }
      return checkStoredToken;
    }()
    /**
     * Устанавливает данные аутентификации
     */
    )
  }, {
    key: "setAuthData",
    value: function setAuthData(token, userData) {
      this.isAuthenticated = true;
      this.userData = userData;
      localStorage.setItem(this.tokenKey, token);
      localStorage.setItem(this.userDataKey, JSON.stringify(userData));

      // Устанавливаем токен в cookie для серверных запросов
      document.cookie = "auth_token=".concat(token, "; path=/; max-age=3600; samesite=strict");
      this.updateUI();
    }

    /**
     * Очищает данные аутентификации
     */
  }, {
    key: "clearAuthData",
    value: function clearAuthData() {
      this.isAuthenticated = false;
      this.userData = null;
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userDataKey);

      // Удаляем cookie
      document.cookie = 'auth_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
      this.updateUI();
    }

    /**
     * Выполняет выход из системы
     */
  }, {
    key: "logout",
    value: (function () {
      var _logout = _asyncToGenerator(/*#__PURE__*/_regenerator().m(function _callee4() {
        var identityServerUrl;
        return _regenerator().w(function (_context4) {
          while (1) switch (_context4.n) {
            case 0:
              this.clearAuthData();
              this.showNotification('Вы вышли из системы', 'info');

              // Перенаправляем на страницу входа IdentityServer
              identityServerUrl = window.location.protocol + '//' + window.location.hostname + ':55674';
              window.location.href = "".concat(identityServerUrl, "/Auth/Login");
            case 1:
              return _context4.a(2);
          }
        }, _callee4, this);
      }));
      function logout() {
        return _logout.apply(this, arguments);
      }
      return logout;
    }()
    /**
     * Получает токен для API запросов
     */
    )
  }, {
    key: "getAuthToken",
    value: function getAuthToken() {
      return localStorage.getItem(this.tokenKey);
    }

    /**
     * Создает заголовки для авторизованных запросов
     */
  }, {
    key: "getAuthHeaders",
    value: function getAuthHeaders() {
      var token = this.getAuthToken();
      return token ? {
        'Authorization': "Bearer ".concat(token)
      } : {};
    }

    /**
     * Обновляет UI в зависимости от состояния авторизации
     */
  }, {
    key: "updateUI",
    value: function updateUI() {
      var _this = this;
      // Показываем/скрываем элементы для авторизованных пользователей
      var authElements = document.querySelectorAll('[data-auth-required]');
      var guestElements = document.querySelectorAll('[data-guest-only]');
      authElements.forEach(function (element) {
        element.style.display = _this.isAuthenticated ? 'block' : 'none';
      });
      guestElements.forEach(function (element) {
        element.style.display = _this.isAuthenticated ? 'none' : 'block';
      });

      // Обновляем информацию о пользователе
      if (this.isAuthenticated && this.userData) {
        var userEmailElements = document.querySelectorAll('[data-user-email]');
        var userIdElements = document.querySelectorAll('[data-user-id]');
        userEmailElements.forEach(function (element) {
          element.textContent = _this.userData.email;
        });
        userIdElements.forEach(function (element) {
          element.textContent = _this.userData.userId;
        });
      }

      // Обновляем состояние кнопок
      var loginButtons = document.querySelectorAll('[data-login-btn]');
      var logoutButtons = document.querySelectorAll('[data-logout-btn]');
      loginButtons.forEach(function (btn) {
        btn.style.display = _this.isAuthenticated ? 'none' : 'inline-block';
      });
      logoutButtons.forEach(function (btn) {
        btn.style.display = _this.isAuthenticated ? 'inline-block' : 'none';
        btn.onclick = function () {
          return _this.logout();
        };
      });

      // Генерируем кастомное событие для других скриптов
      var authEvent = new CustomEvent('authStateChanged', {
        detail: {
          isAuthenticated: this.isAuthenticated,
          userData: this.userData
        }
      });
      document.dispatchEvent(authEvent);
    }

    /**
     * Удаляет код аутентификации из URL
     */
  }, {
    key: "clearAuthCodeFromUrl",
    value: function clearAuthCodeFromUrl() {
      var url = new URL(window.location);
      url.searchParams["delete"]('auth');
      window.history.replaceState(null, '', url);
    }

    /**
     * Показывает уведомление пользователю
     */
  }, {
    key: "showNotification",
    value: function showNotification(message) {
      var type = arguments.length > 1 && arguments[1] !== undefined ? arguments[1] : 'info';
      // Создаем простое уведомление
      var notification = document.createElement('div');
      notification.className = "notification notification-".concat(type);
      notification.textContent = message;
      notification.style.cssText = "\n            position: fixed;\n            top: 20px;\n            right: 20px;\n            padding: 12px 20px;\n            border-radius: 4px;\n            color: white;\n            z-index: 10000;\n            font-weight: 500;\n            box-shadow: 0 4px 8px rgba(0,0,0,0.1);\n            ".concat(type === 'success' ? 'background-color: #10B981;' : '', "\n            ").concat(type === 'error' ? 'background-color: #EF4444;' : '', "\n            ").concat(type === 'info' ? 'background-color: #3B82F6;' : '', "\n        ");
      document.body.appendChild(notification);

      // Автоматически удаляем через 5 секунд
      setTimeout(function () {
        notification.remove();
      }, 5000);

      // Добавляем возможность закрытия по клику
      notification.onclick = function () {
        return notification.remove();
      };
    }
  }]);
}(); // Глобальный экземпляр менеджера аутентификации
window.authManager = new AuthManager();

// Полезные глобальные функции
window.isAuthenticated = function () {
  return window.authManager.isAuthenticated;
};
window.getCurrentUser = function () {
  return window.authManager.userData;
};
window.logout = function () {
  return window.authManager.logout();
};
})();

// This entry needs to be wrapped in an IIFE because it needs to be in strict mode.
(() => {
"use strict";
var __webpack_exports__ = {};
/*!***********************************!*\
  !*** ./wwwroot/css/variables.css ***!
  \***********************************/
__webpack_require__.r(__webpack_exports__);
// extracted by mini-css-extract-plugin

})();

// This entry needs to be wrapped in an IIFE because it needs to be in strict mode.
(() => {
"use strict";
var __webpack_exports__ = {};
/*!******************************!*\
  !*** ./wwwroot/css/site.css ***!
  \******************************/
__webpack_require__.r(__webpack_exports__);
// extracted by mini-css-extract-plugin

})();

// This entry needs to be wrapped in an IIFE because it needs to be in strict mode.
(() => {
"use strict";
var __webpack_exports__ = {};
/*!**********************************!*\
  !*** ./wwwroot/css/features.css ***!
  \**********************************/
__webpack_require__.r(__webpack_exports__);
// extracted by mini-css-extract-plugin

})();

// This entry needs to be wrapped in an IIFE because it needs to be in strict mode.
(() => {
"use strict";
/*!*****************************!*\
  !*** ./wwwroot/css/btn.css ***!
  \*****************************/
__webpack_require__.r(__webpack_exports__);
// extracted by mini-css-extract-plugin

})();

/******/ })()
;
//# sourceMappingURL=app.js.map