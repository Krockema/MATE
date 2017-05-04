define([
	"../validate"
], function( validate ) {

return function( value, name, check, expected ) {
	validate(
		"E_INVALId_PAR_TYPE",
		"Invalid `{name}` parameter ({value}). {expected} expected.",
		check,
		{
			expected: expected,
			name: name,
			value: value
		}
	);
};

});
