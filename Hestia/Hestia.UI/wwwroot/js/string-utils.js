// String utility functions for template replacement
window.stringUtils = {
    /**
     * Replaces [anything] placeholders in a string with values from an array
     * @param {string} template - The template string with [anything] placeholders
     * @param {Array} values - Array of values to replace placeholders with
     * @returns {string} - The string with placeholders replaced
     * 
     * Example: fillTemplate("abc[#]def[?]ghi[whatever]", [1, 2, 3]) 
     *          returns "abc[1]def[2]ghi[3]"
     */
    fillTemplate: function(template, values) {
        if (!template || !Array.isArray(values)) {
            return template;
        }
        
        let result = template;
        let valueIndex = 0;
        
        // Replace each [anything] with the corresponding value from the array
        result = result.replace(/\[[^\]]*\]/g, function(match) {
            if (valueIndex < values.length) {
                return '[' + values[valueIndex++] + ']';
            }
            return match; // If no more values, leave placeholder unchanged
        });
        
        return result;
    }
};