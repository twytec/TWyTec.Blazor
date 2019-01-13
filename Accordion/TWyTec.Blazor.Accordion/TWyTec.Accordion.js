
window.twytecAccordionGetPanelHeight = (id) => {
    let el = document.getElementById(id);
    return el.scrollHeight;
};

window.twytecAccordionSetPanelHeight = (id) => {
    let el = document.getElementById(id);
    let ac = el.getAttribute('data-AnimateClass');
    el.classList.remove(ac);
    el.style.maxHeight = el.scrollHeight + 'px';
    el.classList.add(ac);
    return true;
};