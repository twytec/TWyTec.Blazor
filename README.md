# TWyTec.Blazor

## [Accordion](https://github.com/twytec/TWyTec.Blazor/tree/master/Accordion)

@using TWyTec.Blazor

@addTagHelper *, TWyTec.Blazor.Accordion

```html
<Accordion>
  <AccordionItem Header="Accordion 1">
    Text
  </AccordionItem>
</Accordion>
```

## [ContentDialog](https://github.com/twytec/TWyTec.Blazor/tree/master/contentdialog)

@using TWyTec.Blazor

@addTagHelper *, TWyTec.Blazor.ContentDialog

```html
<ContentDialog>
    <div style="text-align: center;">
        <p>Hello World</p>
        <button onclick="@CloseContentDialog">
            Close
        </button>
    </div>
</ContentDialog>
```
