(function () {
    htmx.defineExtension('rename', {
        onEvent: function (name, evt) {
            if (name !== "htmx:configRequest")
                return;

            const parent = evt.detail.elt;
            const renameAttr = parent.getAttribute('hx-rename');
            if (!renameAttr)
                return;

            const renameMap = {};
            renameAttr.split(',').forEach(pair => {
                const [k, v] = pair.split(':');
                renameMap[k] = v;
            });

            const newParams = {};
            const makeArray = (value) => Array.isArray(value) ? value : [value];
            for (const [key, value] of Object.entries(evt.detail.parameters)) {
                const targetKey = renameMap[key] || key;
                newParams[targetKey] = (newParams[targetKey] || []).concat(makeArray(value));
            }
            
            const unArray = (value) => Array.isArray(value) && value.length == 1 ? value[0] : value;
            
            // Replace parameters with transformed ones
            evt.detail.parameters = Object.fromEntries(
                Object.entries(newParams).map(([key, value]) => [key, unArray(value)])
            );
        }
    });
})();
