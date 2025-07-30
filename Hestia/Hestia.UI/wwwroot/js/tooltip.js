window.HestiaUI = window.HestiaUI || {};
window.HestiaUI.positionTooltip = function(options) {
    const { trigger, tooltip, position, offset, padding } = options;
    FloatingUIDOM.computePosition(trigger, tooltip, {
        placement: position,
        middleware: [
            FloatingUIDOM.offset(offset),
            FloatingUIDOM.flip(),
            FloatingUIDOM.shift({ padding: padding}),
            FloatingUIDOM.size({
                padding: padding,
                apply({ availableWidth, availableHeight, elements }) {
                    Object.assign(elements.floating.style, {
                        maxWidth: `${Math.max(0, availableWidth)}px`,
                        maxHeight: `${Math.max(0, availableHeight)}px`,
                    });
                }
            }),
        ]
    }).then(({ x, y }) => {
        Object.assign(tooltip.style, {
            left: `${x}px`,
            top: `${y}px`
        });
    });
};

window.HestiaUI.autoPositionTooltip = function(options) {
    function updatePosition() {
        window.HestiaUI.positionTooltip(options);
    }
    return FloatingUIDOM.autoUpdate(options.trigger, options.tooltip, updatePosition);
};