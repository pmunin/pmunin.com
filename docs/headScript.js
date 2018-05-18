//script should be injected in the head of blogger's template HTML

/**
 * 
 * 
 * @param {string} fileUrl 
 */
function githubFile(fileUrl)
{
    //see documentation here: http://gist-it.appspot.com/
    // example: <script src="https://gist-it.appspot.com/https://github.com/pmunin/homepage/blob/master/views/layout.ejs?footer=minimal"></script>

    //console.log("rendering code of github file:"+fileUrl);
    // if(fileUrl.startsWith(""))
    // if(fileUrl.indexOf("github.com")==-1)
    document.write("<script src='https://gist-it.appspot.com/"+fileUrl+"'></script>")
    // else 
    //     document.write("<script src='https://gist-it.appspot.com/https://github.com/"+fileUrl+"'></script>")
}