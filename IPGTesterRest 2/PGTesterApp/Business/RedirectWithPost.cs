using System;
using System.Collections.Specialized;
using System.Web.UI;

namespace PGTesterApp.Business
{
    public class RedirectWithPost
    {
        public static void Redirect(Control updatePanelOrThis, string destinationUrl,
                                   NameValueCollection data)
        {
            string strForm = PreparePOSTForm(destinationUrl, data);
            ScriptManager.RegisterStartupScript(updatePanelOrThis, updatePanelOrThis.GetType(), (new Guid()).ToString(),
                        strForm, false);
        }

        public static void PageRedirect(Page page, string destinationUrl,
                                   NameValueCollection data)
        {
            string strForm = PreparePOSTForm(destinationUrl, data);
            ScriptManager.RegisterStartupScript(page, page.GetType(), (new Guid()).ToString(),
                        strForm, false);
        }

        private static String PreparePOSTForm(string url, NameValueCollection data)
        {
            string jscriptString = "<script language=" + "\"" + "javascript" + "\"" + " type=" + "\"" + "text/javascript" + "\"" + ">" +
            "function postToPage() " + "{" + "var form = document.createElement(" + "\"" + "form" + "\"" + ");" +
            "form.setAttribute(" + "\"" + "method" + "\"" + ", " + "\"" + "POST" + "\"" + ");" +
            "form.setAttribute(" + "\"" + "action" + "\"" + ", " + "\"" + url + "\"" + ");" +
            "form.setAttribute(" + "\"" + "target" + "\"" + ", " + "\"" + "_self" + "\"" + ");";

            int counter = 0;
            if (data != null && data.Count > 0)
            {
                foreach (string key in data)
                {
                    jscriptString += "var hiddenField" + counter.ToString() + " = document.createElement(" + "\"" + "input" + "\"" + ");" +
                "hiddenField" + counter.ToString() + ".setAttribute(" + "\"" + "name" + "\"" + ", " + "\"" + key + "\"" + ");" +
                "hiddenField" + counter.ToString() + ".setAttribute(" + "\"" + "value" + "\"" + ", " + "\"" + data[key] + "\"" + ");" +
                "form.appendChild(hiddenField" + counter.ToString() + ");";
                    counter++;
                }
            }

            jscriptString += "document.body.appendChild(form);form.submit();document.body.removeChild(form);}postToPage();</script>";
            return jscriptString;
        }
    }
}
