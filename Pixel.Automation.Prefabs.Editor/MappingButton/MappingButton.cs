using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Prefabs.Editor
{
    public abstract class MappingButton : UserControl
    {

        public static readonly DependencyProperty PrefabVersionProperty = DependencyProperty.Register("PrefabVersion", typeof(PrefabVersion), typeof(MappingButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public PrefabVersion PrefabVersion
        {
            get { return (PrefabVersion)GetValue(PrefabVersionProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }


        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(MappingButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }


        public static readonly DependencyProperty AssignFromProperty = DependencyProperty.Register("AssignFrom", typeof(Type), typeof(MappingButton),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Type AssignFrom
        {
            get { return (Type)GetValue(AssignFromProperty); }
            set { SetValue(AssignFromProperty, value); }
        }

        public static readonly DependencyProperty AssignToProperty = DependencyProperty.Register("AssignTo", typeof(Type), typeof(MappingButton),
                                                                                         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Type AssignTo
        {
            get { return (Type)GetValue(AssignToProperty); }
            set { SetValue(AssignToProperty, value); }
        }

        public static readonly DependencyProperty EntityManagerProperty = DependencyProperty.Register("EntityManager", typeof(EntityManager), typeof(MappingButton),
                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public EntityManager EntityManager
        {
            get { return (EntityManager)GetValue(EntityManagerProperty); }
            set { SetValue(EntityManagerProperty, value); }
        }
    }
}
