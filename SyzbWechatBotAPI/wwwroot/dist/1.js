webpackJsonp([1],{

/***/ 209:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

__webpack_require__(226);

var _react = __webpack_require__(1);

var React = _interopRequireWildcard(_react);

var _isomorphicFetch = __webpack_require__(213);

var _isomorphicFetch2 = _interopRequireDefault(_isomorphicFetch);

var _Jumbotron = __webpack_require__(224);

var _Jumbotron2 = _interopRequireDefault(_Jumbotron);

var _Label = __webpack_require__(222);

var _Label2 = _interopRequireDefault(_Label);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } else { var newObj = {}; if (obj != null) { for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) newObj[key] = obj[key]; } } newObj.default = obj; return newObj; } }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var News = function (_React$Component) {
    _inherits(News, _React$Component);

    function News() {
        _classCallCheck(this, News);

        var _this = _possibleConstructorReturn(this, (News.__proto__ || Object.getPrototypeOf(News)).call(this));

        _this.state = {
            news: {}, loading: true
        };
        return _this;
    }

    _createClass(News, [{
        key: 'componentDidMount',
        value: function componentDidMount() {
            var _this2 = this;

            var id = this.props.match.params.id;

            (0, _isomorphicFetch2.default)('/api/News?id=' + id).then(function (response) {
                return response.json();
            }).then(function (json) {
                _this2.setState({
                    news: json, loading: false
                });
            });
        }
    }, {
        key: 'render',
        value: function render() {
            var news = this.state.news;

            return React.createElement(
                _Jumbotron2.default,
                null,
                React.createElement(
                    'h1',
                    null,
                    news.title
                ),
                React.createElement(
                    _Label2.default,
                    { bsStyle: 'info' },
                    news.time
                ),
                React.createElement(
                    'p',
                    null,
                    news.text
                )
            );
        }
    }]);

    return News;
}(React.Component);

exports.default = News;

/***/ }),

/***/ 210:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(10);

/***/ }),

/***/ 213:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(312);

/***/ }),

/***/ 222:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


exports.__esModule = true;

var _values = __webpack_require__(223);

var _values2 = _interopRequireDefault(_values);

var _extends2 = __webpack_require__(30);

var _extends3 = _interopRequireDefault(_extends2);

var _objectWithoutProperties2 = __webpack_require__(60);

var _objectWithoutProperties3 = _interopRequireDefault(_objectWithoutProperties2);

var _classCallCheck2 = __webpack_require__(61);

var _classCallCheck3 = _interopRequireDefault(_classCallCheck2);

var _possibleConstructorReturn2 = __webpack_require__(62);

var _possibleConstructorReturn3 = _interopRequireDefault(_possibleConstructorReturn2);

var _inherits2 = __webpack_require__(63);

var _inherits3 = _interopRequireDefault(_inherits2);

var _classnames = __webpack_require__(64);

var _classnames2 = _interopRequireDefault(_classnames);

var _react = __webpack_require__(1);

var _react2 = _interopRequireDefault(_react);

var _bootstrapUtils = __webpack_require__(65);

var _StyleConfig = __webpack_require__(68);

function _interopRequireDefault(obj) {
  return obj && obj.__esModule ? obj : { default: obj };
}

var Label = function (_React$Component) {
  (0, _inherits3.default)(Label, _React$Component);

  function Label() {
    (0, _classCallCheck3.default)(this, Label);
    return (0, _possibleConstructorReturn3.default)(this, _React$Component.apply(this, arguments));
  }

  Label.prototype.hasContent = function hasContent(children) {
    var result = false;

    _react2.default.Children.forEach(children, function (child) {
      if (result) {
        return;
      }

      if (child || child === 0) {
        result = true;
      }
    });

    return result;
  };

  Label.prototype.render = function render() {
    var _props = this.props,
        className = _props.className,
        children = _props.children,
        props = (0, _objectWithoutProperties3.default)(_props, ['className', 'children']);

    var _splitBsProps = (0, _bootstrapUtils.splitBsProps)(props),
        bsProps = _splitBsProps[0],
        elementProps = _splitBsProps[1];

    var classes = (0, _extends3.default)({}, (0, _bootstrapUtils.getClassSet)(bsProps), {

      // Hack for collapsing on IE8.
      hidden: !this.hasContent(children)
    });

    return _react2.default.createElement('span', (0, _extends3.default)({}, elementProps, { className: (0, _classnames2.default)(className, classes) }), children);
  };

  return Label;
}(_react2.default.Component);

exports.default = (0, _bootstrapUtils.bsClass)('label', (0, _bootstrapUtils.bsStyles)([].concat((0, _values2.default)(_StyleConfig.State), [_StyleConfig.Style.DEFAULT, _StyleConfig.Style.PRIMARY]), _StyleConfig.Style.DEFAULT, Label));
module.exports = exports['default'];

/***/ }),

/***/ 223:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(0))(30);

/***/ }),

/***/ 224:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


exports.__esModule = true;

var _extends2 = __webpack_require__(30);

var _extends3 = _interopRequireDefault(_extends2);

var _objectWithoutProperties2 = __webpack_require__(60);

var _objectWithoutProperties3 = _interopRequireDefault(_objectWithoutProperties2);

var _classCallCheck2 = __webpack_require__(61);

var _classCallCheck3 = _interopRequireDefault(_classCallCheck2);

var _possibleConstructorReturn2 = __webpack_require__(62);

var _possibleConstructorReturn3 = _interopRequireDefault(_possibleConstructorReturn2);

var _inherits2 = __webpack_require__(63);

var _inherits3 = _interopRequireDefault(_inherits2);

var _react = __webpack_require__(1);

var _react2 = _interopRequireDefault(_react);

var _classnames = __webpack_require__(64);

var _classnames2 = _interopRequireDefault(_classnames);

var _elementType = __webpack_require__(210);

var _elementType2 = _interopRequireDefault(_elementType);

var _bootstrapUtils = __webpack_require__(65);

function _interopRequireDefault(obj) {
  return obj && obj.__esModule ? obj : { default: obj };
}

var propTypes = {
  componentClass: _elementType2.default
};

var defaultProps = {
  componentClass: 'div'
};

var Jumbotron = function (_React$Component) {
  (0, _inherits3.default)(Jumbotron, _React$Component);

  function Jumbotron() {
    (0, _classCallCheck3.default)(this, Jumbotron);
    return (0, _possibleConstructorReturn3.default)(this, _React$Component.apply(this, arguments));
  }

  Jumbotron.prototype.render = function render() {
    var _props = this.props,
        Component = _props.componentClass,
        className = _props.className,
        props = (0, _objectWithoutProperties3.default)(_props, ['componentClass', 'className']);

    var _splitBsProps = (0, _bootstrapUtils.splitBsProps)(props),
        bsProps = _splitBsProps[0],
        elementProps = _splitBsProps[1];

    var classes = (0, _bootstrapUtils.getClassSet)(bsProps);

    return _react2.default.createElement(Component, (0, _extends3.default)({}, elementProps, { className: (0, _classnames2.default)(className, classes) }));
  };

  return Jumbotron;
}(_react2.default.Component);

Jumbotron.propTypes = propTypes;
Jumbotron.defaultProps = defaultProps;

exports.default = (0, _bootstrapUtils.bsClass)('jumbotron', Jumbotron);
module.exports = exports['default'];

/***/ }),

/***/ 225:
/***/ (function(module, exports, __webpack_require__) {

exports = module.exports = __webpack_require__(66)(false);
// imports


// module
exports.push([module.i, ".jumbotron {\n  background-color: none;\n}\n", ""]);

// exports


/***/ }),

/***/ 226:
/***/ (function(module, exports, __webpack_require__) {

// style-loader: Adds some css to the DOM by adding a <style> tag

// load the styles
var content = __webpack_require__(225);
if(typeof content === 'string') content = [[module.i, content, '']];
// Prepare cssTransformation
var transform;

var options = {"hmr":true}
options.transform = transform
// add the styles to the DOM
var update = __webpack_require__(67)(content, options);
if(content.locals) module.exports = content.locals;
// Hot Module Replacement
if(true) {
	// When the styles change, update the <style> tags
	if(!content.locals) {
		module.hot.accept(225, function() {
			var newContent = __webpack_require__(225);
			if(typeof newContent === 'string') newContent = [[module.i, newContent, '']];
			update(newContent);
		});
	}
	// When the module is disposed, remove the <style> tags
	module.hot.dispose(function() { update(); });
}

/***/ })

});
//# sourceMappingURL=1.js.map