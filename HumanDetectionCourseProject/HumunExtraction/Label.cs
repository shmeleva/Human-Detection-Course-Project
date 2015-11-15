﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HumanDetection
{
    internal class Label
    {
        #region Публичные свойства

        public int Name { get; set; }
        public Label Root { get; set; }
        public int Rank { get; set; }

        #endregion

        #region Конструктор

        public Label(int Name)
        {
            this.Name = Name;
            this.Root = this;
            this.Rank = 0;
        }

        #endregion

        #region Публичные методы

        internal Label GetRoot()
        {
            if (this.Root != this)
            {
                this.Root = this.Root.GetRoot();
            }

            return this.Root;
        }

        internal void Join(Label root2)
        {
            if (root2.Rank < this.Rank)//is the rank of Root2 less than that of Root1 ?
            {
                root2.Root = this;//yes! then Root1 is the parent of Root2 (since it has the higher rank)
            }
            else //rank of Root2 is greater than or equal to that of Root1
            {
                this.Root = root2;//make Root2 the parent
                if (this.Rank == root2.Rank)//both ranks are equal ?
                {
                    root2.Rank++;//increment Root2, we need to reach a single root for the whole tree
                }
            }
        }

        #endregion
    }
}