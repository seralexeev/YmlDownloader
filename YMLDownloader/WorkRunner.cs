using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static YMLDownloader.YmlStreamProcessor;

namespace YMLDownloader
{
    // Лучше убрать прогресс отсюда
    // нам возможно нужно будет юзать это без консоли 
    // и надо абстрагироваться от такого нотифаера
    public class WorkRunner
    {
        public async Task<IEnumerable<YmlResult>> Run(Config c)
        {
            var resources = c.ResourceProvider.GetResources().ToList();

            var ymls = new LinkedList<YmlResult>();

            using (var streamProvider = new YmlStreamProvider(c.ConcurrencyDegree, retryPolicy: c.RetryPolicy))
            using (var pbar = new ProgressBar(resources.Count, "Overal progress", c.OveralProgress))
            {
                try
                {
                    var throttler = new SemaphoreSlim(c.ConcurrencyDegree, c.ConcurrencyDegree);
                    var tasks = new List<Task>(resources.Count);
                    foreach (var url in resources)
                    {
                        await throttler.WaitAsync().ConfigureAwait(false);

                        tasks.Add(Task.Run(async () =>
                        {
                            using (var tpbar = pbar.Spawn(1, $"Connecting... {url}", c.SubProgress))
                            {
                                var proc = new YmlStreamProcessor(
                                    url, c.YmlStreamReader, c.XmlProductParser, c.Validator, c.Logger,
                                    c.ProductSaver, c.FlushBufferSize);

                                YmlResult result;
                                try
                                {
                                    using (var yml = await streamProvider.GetYmlStream(proc.Url).ConfigureAwait(false))
                                    {
                                        tpbar.UpdateMaxTicks(yml.GetMaxTicks());
                                        tpbar.UpdateMessage(proc.Url);

                                        if (yml.ContentLength != null)
                                            yml.OnPositionChanged += (sender, e) => tpbar.Tick();

                                        result = proc.Process(yml);
                                    }
                                }
                                catch (Exception e)
                                {
                                    tpbar.UpdateMessage($"Failed {e.Message}");

                                    result = proc.FromException(e);
                                }
                                finally
                                {
                                    throttler.Release();
                                    pbar.Tick();
                                    //if (yml.ContentLength == null)
                                    //    tpbar.Tick();
                                    // можем не получить контент ленс
                                }

                                ymls.AddLast(result);
                            }
                        }));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }

            return ymls;
        }

        public class Config
        {
            public ResourceProvider ResourceProvider { get; set; }
            public XmlProductParser XmlProductParser { get; set; }
            public IProductSaver ProductSaver { get; set; }
            public YmlStreamReader YmlStreamReader { get; set; }
            public Validator Validator { get; set; }
            public ILogger Logger { get; set; }

            public int ConcurrencyDegree { get; set; }
            public int FlushBufferSize { get; set; }
            public int RetryPolicy { get; set; }
            public ProgressBarOptions OveralProgress { get; set; }
            public ProgressBarOptions SubProgress { get; set; }
        }
    }
}
