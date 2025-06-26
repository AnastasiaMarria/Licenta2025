// Chart.js helper functions for Blazor components
let chartInstances = {};

window.createLineChart = function (canvasId, chartData, options)
{
    try
    {
        console.log(`🔍 DEBUG: Creating chart ${canvasId}`);
        console.log(`🔍 DEBUG: Chart.js available:`, typeof Chart !== 'undefined');
        console.log(`🔍 DEBUG: ChartData:`, chartData);

        const canvas = document.getElementById(canvasId);
        if (!canvas)
        {
            console.error(`❌ Canvas with id '${canvasId}' not found in DOM`);
            console.log(`🔍 DEBUG: Available elements:`, Array.from(document.querySelectorAll('canvas')).map(c => c.id));
            return;
        }

        console.log(`✅ Canvas found:`, canvas);
        console.log(`🔍 DEBUG: Canvas dimensions:`, canvas.width, 'x', canvas.height);

        // Destroy existing chart if it exists
        if (chartInstances[canvasId])
        {
            console.log(`🔄 Destroying existing chart ${canvasId}`);
            chartInstances[canvasId].destroy();
            delete chartInstances[canvasId];
        }

        // Get canvas context
        const ctx = canvas.getContext('2d');
        console.log(`✅ Canvas context obtained:`, ctx);

        // Create new chart
        console.log(`🚀 Creating new Chart instance for ${canvasId}`);
        chartInstances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: chartData,
            options: options
        });

        console.log(`✅ Chart '${canvasId}' created successfully!`);
        console.log(`🔍 DEBUG: Chart instance:`, chartInstances[canvasId]);

        // Force a render
        chartInstances[canvasId].update();

    } catch (error)
    {
        console.error(`❌ Error creating chart '${canvasId}':`, error);
        console.error(`❌ Error stack:`, error.stack);
    }
};

window.updateLineChart = function (canvasId, newData)
{
    try
    {
        const chart = chartInstances[canvasId];
        if (!chart)
        {
            console.error(`Chart with id '${canvasId}' not found`);
            return;
        }

        chart.data = newData;
        chart.update('active');
        console.log(`Chart '${canvasId}' updated successfully`);
    } catch (error)
    {
        console.error(`Error updating chart '${canvasId}':`, error);
    }
};

window.destroyChart = function (canvasId)
{
    try
    {
        if (chartInstances[canvasId])
        {
            chartInstances[canvasId].destroy();
            delete chartInstances[canvasId];
            console.log(`Chart '${canvasId}' destroyed successfully`);
        }
    } catch (error)
    {
        console.error(`Error destroying chart '${canvasId}':`, error);
    }
};

// Clean up charts when page unloads
window.addEventListener('beforeunload', function ()
{
    Object.keys(chartInstances).forEach(chartId =>
    {
        destroyChart(chartId);
    });
});

console.log('Chart.js helper functions loaded successfully'); 