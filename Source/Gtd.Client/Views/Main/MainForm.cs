﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gtd.CoreDomain;
using Gtd.Shell;
using Gtd.Shell.Projections;

namespace Gtd.Client
{
    public partial class MainForm : Form, IMainDock
    {
        readonly IPublisher _sink;

        public MainForm(IPublisher sink)
        {
            _sink = sink;
            InitializeComponent();



            Load += (sender, args) => sink.Publish(new FormLoading());
            //Shown += (sender, args) => sink.Publish(new FormLoaded());
            captureToolStripMenuItem.Click += (sender, args) => _sink.Publish(new CaptureThoughtClicked());
            projectToolStripMenuItem.Click += (sender, args) =>
                {
                    var c = DefineProjectForm.TryGetUserInput(this);
                    if (null != c) _sink.Publish(new RequestDefineNewProject(c));
                };


            // wire in all controllers
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // when we are loading the form for the 1st time
            


        }

        IDictionary<string, UserControl> _panels = new Dictionary<string, UserControl>();
        string _activePanel = null;

        public void RegisterDock(UserControl control, string key)
        {
            
            var panel = splitContainer1.Panel2;

            panel.Invoke(new Action(() =>
                {
                    control.Visible = false;
                    control.Dock = DockStyle.Fill;
                    panel.Controls.Add(control);

                    _panels.Add(key, control);
                }));

            
        }

        public void SwitchTo(string key)
        {
            var panel = splitContainer1.Panel2;
            panel.Invoke(new Action(() =>
                {
                    if (_activePanel != null)
                    {
                        _panels[_activePanel].Visible = false;
                    }
                    _activePanel = key;
                    _panels[_activePanel].Visible = true;
                }));
        }

        
    }

    public static class ExtendControl
    {
        public static void Sync(this Control self, Action act)
        {
            if (self.InvokeRequired)
            {
                self.Invoke(act);
            }
            else
            {
                act();
            }
        }
    }

    public sealed class RequestMoveThoughtsToProject : Message
    {
        public readonly ThoughtId[] Thoughts;
        public readonly ProjectId Project;

        public RequestMoveThoughtsToProject(ThoughtId[] thoughts, ProjectId project)
        {
            Thoughts = thoughts;
            Project = project;
        }
    }

    public sealed class RequestDefineNewProject : Message
    {
        public readonly string Outcome;

        public RequestDefineNewProject(string outcome)
        {
            Outcome = outcome;
        }
    }

    public sealed class RequestCaptureThought : Message
    {
        public readonly string Thought;

        public RequestCaptureThought(string thought)
        {
            Thought = thought;
        }
    }

    public sealed class CaptureThoughtClicked : Message
    {
        
    }




    public sealed class RequestArchiveThought : Message
    {
        public readonly ThoughtId Id;

        public RequestArchiveThought(ThoughtId id)
        {
            Id = id;
        }
    }



    public enum AppState
    {
        Loading,
        InboxView
    }



    



    public enum UIState
    {
        Loading,
        Inbox
    }

    


}