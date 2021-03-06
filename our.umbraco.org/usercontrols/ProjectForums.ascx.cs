﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using uForum.Businesslogic;
using umbraco.presentation.nodeFactory;

namespace our.usercontrols {
    public partial class ProjectForums : System.Web.UI.UserControl {
        protected void Page_Load(object sender, EventArgs e) {
            if (!Page.IsPostBack) {

                int pId = 0;

                if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn()) {
                    
                    Member m = Member.GetCurrentMember();
                    Document d = new Document(pId);

                    if ((int)d.getProperty("owner").Value == m.Id) {
                        holder.Visible = true;

                        
                        rp_forums.DataSource = uForum.Businesslogic.Forum.Forums(pId);
                        rp_forums.DataBind();

                        int fId = 0;

                        if (!string.IsNullOrEmpty(Request.QueryString["forum"]) && int.TryParse(Request.QueryString["forum"], out fId)) {
                        
                            uForum.Businesslogic.Forum f = new Forum(fId);
                            tb_desc.Text = f.Description;
                            tb_name.Text = f.Title;

                            bt_submit.CommandArgument = f.Id.ToString();
                            bt_delete.CommandArgument = f.Id.ToString();

                            bt_submit.CommandName = "edit";

                            ph_add.Visible = true;
                            ph_edit.Visible = true;


                        } else if (!string.IsNullOrEmpty(Request.QueryString["add"])) {
                            ph_add.Visible = true;
                            ph_edit.Visible = false;
                        }
                    }
                }

                
            }
        }

        protected void bindForum(object sender, RepeaterItemEventArgs e) {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item) {
                uForum.Businesslogic.Forum f = (uForum.Businesslogic.Forum)e.Item.DataItem;
                Literal _title = (Literal)e.Item.FindControl("lt_titel");
                Literal _desc = (Literal)e.Item.FindControl("lt_desc");
                Literal _link = (Literal)e.Item.FindControl("lt_link");

                _title.Text = f.Title;
                _desc.Text = f.Description;
                _link.Text = "<a href='?id=" + Request.QueryString["id"] + "&forum=" + f.Id.ToString() + "'>Edit</a>";
            }
        }
        
               
        protected void modifyForum(object sender, CommandEventArgs e) {

            int pId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["id"]) && int.TryParse(Request.QueryString["id"], out pId) && umbraco.library.IsLoggedOn()) {

                Member m = Member.GetCurrentMember();
                Document d = new Document(pId);
                Document fnode = null;

                if (e.CommandName == "edit") {
                    int fId = int.Parse(e.CommandArgument.ToString());
                    fnode = new Document(fId);

                } else if(e.CommandName == "create") {
                    
                    fnode = Document.MakeNew(tb_name.Text, DocumentType.GetByAlias("Forum"), new umbraco.BusinessLogic.User(0), d.Id);
                

                } else if(e.CommandName == "delete"){
                    
                    int fId = int.Parse(e.CommandArgument.ToString());

                    if (Document.IsDocument(fId))
                    {
                        fnode = new Document(fId);
                    }



                    if (fnode != null)
                    {
                        if ((int)d.getProperty("owner").Value == m.Id && fnode.ParentId == d.Id)
                        {
                            fnode.delete();

                            //if still not dead it's because it's in the trashcan and should be deleted once more.
                            if (fnode.ParentId == -20)
                                fnode.delete();


                        }

                        if (fnode.ParentId == -20)
                            fnode.delete();

                        fnode = null;

                    }

                    var forum = new uForum.Businesslogic.Forum(fId);
                    if (forum.Exists)
                    {
                        forum.Delete();
                    }
                    

                   

                
                }
                                
                if (fnode != null && (int)d.getProperty("owner").Value == m.Id && fnode.ParentId == d.Id) {
                    fnode.Text = tb_name.Text;
                    fnode.getProperty("forumDescription").Value = tb_desc.Text;
                    fnode.getProperty("forumAllowNewTopics").Value = true;
                    fnode.Publish(new umbraco.BusinessLogic.User(0));
                    fnode.Save();
                    umbraco.library.UpdateDocumentCache(fnode.Id);
                }

                
                Response.Redirect(Node.GetCurrent().NiceUrl + "?id=" + pId.ToString());
            
            }
        }
    }
}