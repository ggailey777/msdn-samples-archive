//   Copyright 2011 Glenn Gailey

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;

namespace SQLPassBrowser.OratorModelNS
{
    public partial class Session
    {      

        public Nullable<double> SessionDuration
        {
            get
            {
                // If either value is null, we can't return a value.
                if (this.SessionDateTimeStart == null || this.SessionDateTimeEnd == null)
                {
                    return null;
                }

                // Return the calculated timespan value.
                return ((TimeSpan)(this.SessionDateTimeEnd - this.SessionDateTimeStart)).TotalHours;
            }
        }

    }
}
