webpackJsonp([2],{

/***/ 198:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(5);

/***/ }),

/***/ 199:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(6);

/***/ }),

/***/ 200:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(1);

/***/ }),

/***/ 201:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(2);

/***/ }),

/***/ 202:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(3);

/***/ }),

/***/ 203:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(7);

/***/ }),

/***/ 204:
/***/ (function(module, exports, __webpack_require__) {

"use strict";
/* WEBPACK VAR INJECTION */(function(process) {

exports.__esModule = true;
exports._curry = exports.bsSizes = exports.bsStyles = exports.bsClass = undefined;

var _entries = __webpack_require__(210);

var _entries2 = _interopRequireDefault(_entries);

var _extends2 = __webpack_require__(198);

var _extends3 = _interopRequireDefault(_extends2);

exports.prefix = prefix;
exports.getClassSet = getClassSet;
exports.splitBsProps = splitBsProps;
exports.splitBsPropsAndOmit = splitBsPropsAndOmit;
exports.addStyle = addStyle;

var _invariant = __webpack_require__(211);

var _invariant2 = _interopRequireDefault(_invariant);

var _propTypes = __webpack_require__(29);

var _propTypes2 = _interopRequireDefault(_propTypes);

var _StyleConfig = __webpack_require__(208);

function _interopRequireDefault(obj) {
  return obj && obj.__esModule ? obj : { default: obj };
}

function curry(fn) {
  return function () {
    for (var _len = arguments.length, args = Array(_len), _key = 0; _key < _len; _key++) {
      args[_key] = arguments[_key];
    }

    var last = args[args.length - 1];
    if (typeof last === 'function') {
      return fn.apply(undefined, args);
    }
    return function (Component) {
      return fn.apply(undefined, args.concat([Component]));
    };
  };
} // TODO: The publicly exposed parts of this should be in lib/BootstrapUtils.

function prefix(props, variant) {
  var bsClass = (props.bsClass || '').trim();
  !(bsClass != null) ? process.env.NODE_ENV !== 'production' ? (0, _invariant2.default)(false, 'A `bsClass` prop is required for this component') : (0, _invariant2.default)(false) : void 0;
  return bsClass + (variant ? '-' + variant : '');
}

var bsClass = exports.bsClass = curry(function (defaultClass, Component) {
  var propTypes = Component.propTypes || (Component.propTypes = {});
  var defaultProps = Component.defaultProps || (Component.defaultProps = {});

  propTypes.bsClass = _propTypes2.default.string;
  defaultProps.bsClass = defaultClass;

  return Component;
});

var bsStyles = exports.bsStyles = curry(function (styles, defaultStyle, Component) {
  if (typeof defaultStyle !== 'string') {
    Component = defaultStyle;
    defaultStyle = undefined;
  }

  var existing = Component.STYLES || [];
  var propTypes = Component.propTypes || {};

  styles.forEach(function (style) {
    if (existing.indexOf(style) === -1) {
      existing.push(style);
    }
  });

  var propType = _propTypes2.default.oneOf(existing);

  // expose the values on the propType function for documentation
  Component.STYLES = existing;
  propType._values = existing;

  Component.propTypes = (0, _extends3.default)({}, propTypes, {
    bsStyle: propType
  });

  if (defaultStyle !== undefined) {
    var defaultProps = Component.defaultProps || (Component.defaultProps = {});
    defaultProps.bsStyle = defaultStyle;
  }

  return Component;
});

var bsSizes = exports.bsSizes = curry(function (sizes, defaultSize, Component) {
  if (typeof defaultSize !== 'string') {
    Component = defaultSize;
    defaultSize = undefined;
  }

  var existing = Component.SIZES || [];
  var propTypes = Component.propTypes || {};

  sizes.forEach(function (size) {
    if (existing.indexOf(size) === -1) {
      existing.push(size);
    }
  });

  var values = [];
  existing.forEach(function (size) {
    var mappedSize = _StyleConfig.SIZE_MAP[size];
    if (mappedSize && mappedSize !== size) {
      values.push(mappedSize);
    }

    values.push(size);
  });

  var propType = _propTypes2.default.oneOf(values);
  propType._values = values;

  // expose the values on the propType function for documentation
  Component.SIZES = existing;

  Component.propTypes = (0, _extends3.default)({}, propTypes, {
    bsSize: propType
  });

  if (defaultSize !== undefined) {
    if (!Component.defaultProps) {
      Component.defaultProps = {};
    }
    Component.defaultProps.bsSize = defaultSize;
  }

  return Component;
});

function getClassSet(props) {
  var _classes;

  var classes = (_classes = {}, _classes[prefix(props)] = true, _classes);

  if (props.bsSize) {
    var bsSize = _StyleConfig.SIZE_MAP[props.bsSize] || props.bsSize;
    classes[prefix(props, bsSize)] = true;
  }

  if (props.bsStyle) {
    classes[prefix(props, props.bsStyle)] = true;
  }

  return classes;
}

function getBsProps(props) {
  return {
    bsClass: props.bsClass,
    bsSize: props.bsSize,
    bsStyle: props.bsStyle,
    bsRole: props.bsRole
  };
}

function isBsProp(propName) {
  return propName === 'bsClass' || propName === 'bsSize' || propName === 'bsStyle' || propName === 'bsRole';
}

function splitBsProps(props) {
  var elementProps = {};
  (0, _entries2.default)(props).forEach(function (_ref) {
    var propName = _ref[0],
        propValue = _ref[1];

    if (!isBsProp(propName)) {
      elementProps[propName] = propValue;
    }
  });

  return [getBsProps(props), elementProps];
}

function splitBsPropsAndOmit(props, omittedPropNames) {
  var isOmittedProp = {};
  omittedPropNames.forEach(function (propName) {
    isOmittedProp[propName] = true;
  });

  var elementProps = {};
  (0, _entries2.default)(props).forEach(function (_ref2) {
    var propName = _ref2[0],
        propValue = _ref2[1];

    if (!isBsProp(propName) && !isOmittedProp[propName]) {
      elementProps[propName] = propValue;
    }
  });

  return [getBsProps(props), elementProps];
}

/**
 * Add a style variant to a Component. Mutates the propTypes of the component
 * in order to validate the new variant.
 */
function addStyle(Component) {
  for (var _len2 = arguments.length, styleVariant = Array(_len2 > 1 ? _len2 - 1 : 0), _key2 = 1; _key2 < _len2; _key2++) {
    styleVariant[_key2 - 1] = arguments[_key2];
  }

  bsStyles(styleVariant, Component);
}

var _curry = exports._curry = curry;
/* WEBPACK VAR INJECTION */}.call(exports, __webpack_require__(18)))

/***/ }),

/***/ 207:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(10);

/***/ }),

/***/ 208:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


exports.__esModule = true;
var Size = exports.Size = {
  LARGE: 'large',
  SMALL: 'small',
  XSMALL: 'xsmall'
};

var SIZE_MAP = exports.SIZE_MAP = {
  large: 'lg',
  medium: 'md',
  small: 'sm',
  xsmall: 'xs',
  lg: 'lg',
  md: 'md',
  sm: 'sm',
  xs: 'xs'
};

var DEVICE_SIZES = exports.DEVICE_SIZES = ['lg', 'md', 'sm', 'xs'];

var State = exports.State = {
  SUCCESS: 'success',
  WARNING: 'warning',
  DANGER: 'danger',
  INFO: 'info'
};

var Style = exports.Style = {
  DEFAULT: 'default',
  PRIMARY: 'primary',
  LINK: 'link',
  INVERSE: 'inverse'
};

/***/ }),

/***/ 210:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(107);

/***/ }),

/***/ 211:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(13);

/***/ }),

/***/ 214:
/***/ (function(module, exports, __webpack_require__) {

module.exports = (__webpack_require__(5))(312);

/***/ }),

/***/ 223:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _react = __webpack_require__(2);

var React = _interopRequireWildcard(_react);

var _isomorphicFetch = __webpack_require__(214);

var _isomorphicFetch2 = _interopRequireDefault(_isomorphicFetch);

var _Jumbotron = __webpack_require__(224);

var _Jumbotron2 = _interopRequireDefault(_Jumbotron);

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

        _this.state = { loading: true };
        return _this;
    }

    _createClass(News, [{
        key: "componentDidMount",
        value: function componentDidMount() {
            var _this2 = this;

            (0, _isomorphicFetch2.default)("/api/News/" + this.props.id).then(function (response) {
                return response.json();
            }).then(function (json) {
                _this2.setState({
                    news: json, loading: false
                });
            });
        }
    }, {
        key: "render",
        value: function render() {
            var news = this.state.news;

            return React.createElement(
                _Jumbotron2.default,
                null,
                React.createElement(
                    "h1",
                    null,
                    news.title
                ),
                React.createElement(
                    "p",
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

/***/ 224:
/***/ (function(module, exports, __webpack_require__) {

"use strict";


exports.__esModule = true;

var _extends2 = __webpack_require__(198);

var _extends3 = _interopRequireDefault(_extends2);

var _objectWithoutProperties2 = __webpack_require__(199);

var _objectWithoutProperties3 = _interopRequireDefault(_objectWithoutProperties2);

var _classCallCheck2 = __webpack_require__(200);

var _classCallCheck3 = _interopRequireDefault(_classCallCheck2);

var _possibleConstructorReturn2 = __webpack_require__(201);

var _possibleConstructorReturn3 = _interopRequireDefault(_possibleConstructorReturn2);

var _inherits2 = __webpack_require__(202);

var _inherits3 = _interopRequireDefault(_inherits2);

var _react = __webpack_require__(2);

var _react2 = _interopRequireDefault(_react);

var _classnames = __webpack_require__(203);

var _classnames2 = _interopRequireDefault(_classnames);

var _elementType = __webpack_require__(207);

var _elementType2 = _interopRequireDefault(_elementType);

var _bootstrapUtils = __webpack_require__(204);

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

/***/ })

});
//# sourceMappingURL=2.js.map