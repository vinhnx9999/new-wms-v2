using Microsoft.Extensions.Hosting;
using System.Threading.Channels;
using WMSSolution.Shared;

namespace WMSSolution.WMS.IServices;

public class SkuExcelBackgroundService(ISkuService skuService) : BackgroundService
{
    private readonly Channel<List<InputSku>> _queue = Channel.CreateUnbounded<List<InputSku>>();
    private readonly ISkuService _skuService = skuService;

    public async Task QueueImportAsync(List<InputSku> skus)
    {
        await _queue.Writer.WriteAsync(skus);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var skus in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            // chạy import trong background
            //await _skuService.ImportExcelData(skus);
        }
    }
}
