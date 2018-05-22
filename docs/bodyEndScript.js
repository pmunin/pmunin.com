//script should be inserted before body tag closes

//?highlight=text+to+hightlight - highlight exact text (space bars can be replaced with +)
(function(){
    let params = getJsonFromUrl();
    let highlightText = params&&params.highlight;
    if(!highlightText) return;

    //depends on https://markjs.io/ 
    // https://cdnjs.cloudflare.com/ajax/libs/mark.js/8.11.1/mark.min.js should be loaded in the head of template
    if(!Mark) return;
    var markInstance = window.markInstance = new Mark("*");
    markInstance.mark(highlightText, {separateWordSearch:false, iframes:true});
})();