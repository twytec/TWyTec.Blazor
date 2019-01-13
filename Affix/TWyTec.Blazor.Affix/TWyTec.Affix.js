

function twytecIgnoreNextEvent() {
    if (twytecIgnoreNextEvent.isIgnore === true) {
        twytecIgnoreNextEvent.isIgnore = false;
        return true;
    }
    return false;
}

function twytecBtnAffixPaneClick(id) {
    let e = document.getElementById(id);
    twytecIgnoreNextEvent.isIgnore = true;
    e.scrollIntoView();
    changeAffixBtn(e.offsetTop);
    return true;
}

window.addEventListener('scroll', function (e) {
    affixScroll(window.scrollY);
});

window.twytecAffixAfterRender = () => {
    let a = document.getElementsByName('TWyTecAffixContent');
    for (var i = 0; i < a.length; i++) {
        let item = a[i];
        item.addEventListener('scroll', function (e) {
            affixScroll(item.scrollTop);
        });
    }

    return true;
};

function affixScroll(sY) {
    if (twytecIgnoreNextEvent())
        return;
    changeAffixBtn(sY);
}

function changeAffixBtn(sY) {
    let items = document.getElementsByName('AffixContentItem');
    for (var i = 0; i < items.length; i++) {

        let el = items[i];
        let id = el.getAttribute('id');
        let btn = document.getElementById('btn-' + id);

        if (btn !== undefined) {
            let ac = btn.getAttribute('data-activeclass');

            let li = btn.parentElement;
            let ul = null;

            if (li.children.length > 1) {
                let c = li.children[1];
                if (c.localName === "ul")
                    ul = c;
            }

            if (sY + 1 >= el.offsetTop && sY < el.offsetHeight + el.offsetTop) {
                btn.classList.add(ac);

                if (ul !== null) {
                    ul.classList.remove('TWyTecAffixPaneItemHidden');
                }
            }
            else {
                btn.classList.remove(ac);

                if (ul !== null) {
                    ul.classList.add('TWyTecAffixPaneItemHidden');
                }
            }
        }
    }
}

function twytecSetLoadedAffix() {
    changeAffixBtn(window.scrollY);
    return true;
}