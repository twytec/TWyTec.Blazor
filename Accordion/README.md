# Accordion

| Parameter/Property             | Value     |
| ------------------------------ | --------- |
| AccordionClass                 | CSS Class |
| AccordionItemClass             | CSS Class |
| AccordionItemContentClass      | CSS Class |
| AccordionItemButtonClass       | CSS Class |
| AccordionItemButtonActiveClass | CSS Class |
| SelectedIndex                  | int       |

| Methods                             | Description             |
| ----------------------------------- | ----------------------- |
| void ChangeSelectedIndex(int index) | change index and reload |
| int GetSelectedIndex()              | Get current index       |

## Code
```c#
<Accordion>
    <AccordionItem Header="Accordion 1">
        @Text
    </AccordionItem>
    <AccordionItem Header="Accordion 2">
        @Text
    </AccordionItem>
    <AccordionItem Header="Accordion 3">
        @Text
    </AccordionItem>
</Accordion>
```

## Default CSS Classes
```css
.TWyTecAccordion {
    width: 100%;
    height: auto;
}

.TWyTecAccordionItem {
    margin-bottom: 5px;
}

.TWyTecAccordionItemContent {
    display: block;
    overflow: hidden;
    width: 100%;
    border-left: 1px solid gainsboro;
    border-right: 1px solid gainsboro;
    border-bottom: 1px solid gainsboro;
    border-bottom-left-radius: 2px;
    border-bottom-right-radius: 2px;
}

.TWyTecAccordionItemButton {
    background-color: transparent;
    border: 1px solid gainsboro;
    border-radius: 2px;
    cursor: pointer;
    height: 40px;
    width: 100%;
}

    .TWyTecAccordionItemButton:hover {
        color: dodgerblue !important;
        text-decoration: underline;
    }
    .TWyTecAccordionItemButton:focus {
        outline: 0;
    }

.TWyTecAccordionItemButtonActive {
    border-bottom-left-radius: 0px;
    border-bottom-right-radius: 0px;
}
```
