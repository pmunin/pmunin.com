//script should be injected in the head of blogger's template HTML

/**
 * Renders content of the github source file or gist to current document
 * 
 * @param {string} fileUrl 
 */
function githubFile(fileUrl)
{
    //see documentation here: http://gist-it.appspot.com/
    // example: <script src="https://gist-it.appspot.com/https://github.com/pmunin/homepage/blob/master/views/layout.ejs?footer=minimal"></script>

    if(fileUrl.indexOf("gist.github.com")>-1)
        document.write("<script src='"+fileUrl+"'></script>");
    else
        document.write("<script src='https://gist-it.appspot.com/"+fileUrl+"'></script>")
}