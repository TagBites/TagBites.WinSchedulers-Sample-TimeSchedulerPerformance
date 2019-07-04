using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TagBites.WinSchedulers;
using TagBites.WinSchedulers.Descriptors;

namespace TimeSchedulerPerformance
{
    public class SchedulerDataSource : TimeSchedulerDataSource
    {
        private const int RowsCount = 1000;
        private const int MaxTasksCountPerDay = 10;

        private readonly object[] _resources;
        private readonly IDictionary<DateTime, IList<SchedulerTask>> _taskDictionary = new Dictionary<DateTime, IList<SchedulerTask>>();

        public SchedulerDataSource()
        {
            _resources = GenerateResource();
        }


        protected override TimeSchedulerResourceDescriptor CreateResourceDescriptor()
        {
            return new TimeSchedulerResourceDescriptor(typeof(object));
        }
        protected override TimeSchedulerTaskDescriptor CreateTaskDescriptor()
        {
            return new TimeSchedulerTaskDescriptor(typeof(SchedulerTask), nameof(SchedulerTask.Resource), nameof(SchedulerTask.Interval));
        }

        public override IList<object> LoadResources() => _resources;
        public override void LoadContent(TimeSchedulerDataSourceView view)
        {
            var interval = view.Interval;
            var resources = view.Resources;

            foreach (var item in resources)
                view.AddWorkTime(item, interval, Colors.White);

            for (var i = interval.Start; i < interval.End; i = i.AddDays(1))
            {
                if (!_taskDictionary.ContainsKey(i))
                    _taskDictionary.Add(i, GenerateTasks(i).ToList());

                var tasks = _taskDictionary[i];
                foreach (var item in tasks)
                    if (interval.IntersectsWith(item.Interval) && view.Resources.Contains(item.Resource))
                        view.AddTask(item);
            }
        }

        #region Data generation

        private readonly Random _random = new Random();
        private object[] GenerateResource()
        {
            return Enumerable.Range(0, RowsCount).Select(x => $"Employee {x + 1}").Cast<object>().ToArray();
        }
        public IEnumerable<SchedulerTask> GenerateTasks(DateTime dateTime)
        {
            for (var k = 0; k < _resources.Length; k++)
            {
                var element = _resources[k];
                var count = _random.Next(0, MaxTasksCountPerDay);
                for (var j = 0; j < count; j++)
                {
                    var minutes = _random.Next(12 * 60);
                    var length = _random.Next(2 * 60, 11 * 60);

                    yield return new SchedulerTask()
                    {
                        Resource = element,
                        Interval = new DateTimeInterval(dateTime.AddMinutes(minutes), new TimeSpan(0, length, 0))
                    };
                }
            }
        }

        #endregion

        #region Classes

        public class SchedulerTask
        {
            public object Resource { get; set; }
            public DateTimeInterval Interval { get; set; }
            public string Text { get; set; }
        }

        #endregion
    }
}
