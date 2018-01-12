using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

//------------------------------------------------------------------------------//
//                                                                              //
// Author:  Derek Bartram                                                       //
// Date:    23/01/2008                                                          //
// Version: 1.000                                                               //
// Website: http://www.derek-bartram.co.uk                                      //
// Email:   webmaster@derek-bartram.co.uk                                       //
//                                                                              //
// This code is provided on a free to use and/or modify basis for personal work //
// provided that this banner remains in each of the source code files that is   //
// found in the original source. For any publicically available work (source    //
// and/or binaries 'Derek Bartram' and 'http://www.derek-bartram.co.uk' must be //
// credited in both the user documentation, source code (where applicable), and //
// in the user interface (typically Help > About would be appropiate). Please   //
// also contact myself via the provided email address to let me know where and  //
// what my code is being used for; this helps me provide better solutions for   //
// all.                                                                         //
//                                                                              //
// THIS SOURCE AND/OR COMPILED LIBRARY MUST NOT BE USED FOR COMMERCIAL WORK,    //
// including not-for-profit work, without prior consent.                        //
//                                                                              //
// This agreement overrides any other agreements made by any other parties. By  //
// using, viewing, linking, or compiling the included source or binaries you    //
// agree to the terms and conditions as set out here and in any included (if    //
// applicable) license.txt. For commercial licensing please see the web address //
// above or contact myself via email. Thank you.                                //
//                                                                              //
// Please contact me at the above email for further help, information,          //
// comments, suggestions, licensing, or feature requests. Thank you.            //
//                                                                              //
//                                                                              //
//------------------------------------------------------------------------------//

namespace DNBSoft.MSN.ClientController
{
    public class MSNListenableList<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {
        private List<T> innerList = null;

        public MSNListenableList()
        {
            innerList = new List<T>();
        }


        #region IList<T> Members

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);

            if (ElementAdded != null)
            {
                ElementAdded(this, new MSNListenableList<T>.ElementAddedEventArgs<T>(item, index));
            }
        }

        public void RemoveAt(int index)
        {
            T item = innerList[index];
            innerList.RemoveAt(index);

            if (ElementRemoved != null)
            {
                ElementRemoved(this, new MSNListenableList<T>.ElementRemovedEventArgs<T>(item, index));
            }
        }

        public T this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                innerList[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Clear()
        {
            List<T> copyItems = new List<T>();
            if (ElementRemoved != null)
            {
                for (int i = 0; i < innerList.Count; i++)
                {
                    copyItems.Add(innerList[i]);
                }
            }

            innerList.Clear();

            if (ElementRemoved != null)
            {
                for (int i = 0; i < copyItems.Count; i++)
                {
                    ElementRemoved(this, new MSNListenableList<T>.ElementRemovedEventArgs<T>(copyItems[i], i));
                }
            }
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (innerList.Contains(item))
            {
                int index = this.IndexOf(item);
                innerList.Remove(item);

                if (ElementRemoved != null)
                {
                    ElementRemoved(this, new MSNListenableList<T>.ElementRemovedEventArgs<T>(item, index));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Add(T item)
        {
            innerList.Add(item);

            if (ElementAdded != null)
            {
                int index = innerList.IndexOf(item);

                ElementAdded(this, new MSNListenableList<T>.ElementAddedEventArgs<T>(item, index));
            }
        }

        void ICollection<T>.Add(T item)
        {
            innerList.Add(item);

            if (ElementAdded != null)
            {
                int index = innerList.IndexOf(item);

                ElementAdded(this, new MSNListenableList<T>.ElementAddedEventArgs<T>(item, index));
            }
        }

        void ICollection<T>.Clear()
        {
            List<T> copyItems = new List<T>();
            if (ElementRemoved != null)
            {
                for (int i = 0; i < innerList.Count; i++)
                {
                    copyItems.Add(innerList[i]);
                }
            }

            innerList.Clear();

            if (ElementRemoved != null)
            {
                for (int i = 0; i < copyItems.Count; i++)
                {
                    ElementRemoved(this, new MSNListenableList<T>.ElementRemovedEventArgs<T>(copyItems[i], i));
                }
            }
        }

        bool ICollection<T>.Contains(T item)
        {
            return innerList.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return innerList.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<T>.Remove(T item)
        {
            if (innerList.Contains(item))
            {
                int index = this.IndexOf(item);
                innerList.Remove(item);

                if (ElementRemoved != null)
                {
                    ElementRemoved(this, new MSNListenableList<T>.ElementRemovedEventArgs<T>(item, index));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        #region event classes
        public class ElementAddedEventArgs<S> : EventArgs
        {
            private S item;
            private int index = 0;

            public ElementAddedEventArgs(S item, int index)
            {
                this.item = item;
                this.index = index;
            }

            public S Item
            {
                get
                {
                    return item;
                }
                set
                {
                    item = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }
            }
        }

        public class ElementRemovedEventArgs<U> : EventArgs
        {
            private U item;
            private int index = 0;

            public ElementRemovedEventArgs(U item, int index)
            {
                this.item = item;
                this.index = index;
            }

            public U Item
            {
                get
                {
                    return item;
                }
                set
                {
                    item = value;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }
            }
        }
        #endregion

        #region event delegates
        public delegate void ElementAddedDelegate(MSNListenableList<T> sender, ElementAddedEventArgs<T> args);
        public delegate void ElementRemovedDelegate(MSNListenableList<T> sender, ElementRemovedEventArgs<T> args);
        #endregion

        #region event declarations
        public event ElementAddedDelegate ElementAdded;
        public event ElementRemovedDelegate ElementRemoved;
        #endregion
    }
}
