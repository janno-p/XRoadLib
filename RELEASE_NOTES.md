#### 1.0.0-alpha16 - January 20 2016
* Add basic handlers for data and service description requests.
* Fix bugs of previous releases.

#### 1.0.0-alpha15 - January 19 2016
* Refactor to use XName instead of XmlQualifiedName.
* Add deserializer for X-Road standard non-technical fault.
* Add deserializer for standard SOAP fault.
* Parameter name customization for contract parameters only.
* Remove protocol dependency from parameter name selection.
* Remove XRoadSerializer since it's buggy and is not needed by the library.
* Add helper method which deserializes SOAP message and handles faults when present.

#### 1.0.0-alpha14 - January 18 2016
* Add support for X-Road v6 and X-Road messaging protcol v4.0.
* Redesign support for different protocol versions.
* Add interfaces to override default type options.
* Unhandled SOAP header elements as XElement instead of string representation.

#### 1.0.0-alpha13 - January 14 2016
* More annotation elements to types and elements.
* Accept XmlElementAttribute.DataType for parameter elements.
* Type content particle uses always sequence, but deserialization mode may vary.

#### 1.0.0-alpha12 - January 12 2016
* Fix strict parameter deserialization when xml template is not used.
* General request handler for X-Road data requests.
* Refactored serializer cache to deal with one assembly only.
* Bug fixes.

#### 1.0.0-alpha11 - January 11 2016
* Fix service root element name.

#### 1.0.0-alpha10 - January 11 2016
* Fix bug with custom operation type name format.

#### 1.0.0-alpha9 - January 11 2016
* Bug fix.

#### 1.0.0-alpha8 - January 11 2016
* Use `all` particle for non-strict operation types.
* Make `XRoadImportAttribute` protocol specific.
* Common interface for appliable attributes.
* Customize parameter names with global name provider.

#### 1.0.0-alpha7 - January 11 2016
* Allow protocol specific differences for schema import.

#### 1.0.0-alpha6 - January 11 2016
* Customize schema import namespaces and location through attribute parameter.

#### 1.0.0-alpha5 - January 11 2016
* Fix wrong version number in generated service description.
* Add properties to test of operation version limits are specified.

#### 1.0.0-alpha4 - January 11 2016
* Restore separate producer name attribute.
* Hide complexity of operation declarations in producer definition.

#### 1.0.0-alpha3 - January 11 2016
* More configuration options through contract assembly attributes.
* Layout attribute to control content particle and order of elements.

#### 1.0.0-alpha2 - January 7 2016
* Add SOAP specific helper methods.
* Add helper methods to resolve operation contracts.

#### 1.0.0-alpha1 - January 7 2016
* Initial release
* Extract common logic into separate project.
