import { insertScripts } from "./Utils";

let Chart = window["Chart"];

export class ChartjsHelper {
    static install(scriptSource?: string, onload?: (this: GlobalEventHandlers, ev: Event) => any) : Promise<boolean> {
        return new Promise((resolve, reject) => {
            if (Chart) {
                resolve(true);
            } else {
                scriptSource || (scriptSource = 'https://cdn.jsdelivr.net/npm/chart.js');
                return insertScripts(scriptSource, e => {
                    if (onload) onload.call(e)
                    resolve(true);
                });
            }
        });
    }

    static createChart(selector: string, config: ChartjsConfig) {
        
        if (!(Chart || (Chart = window["Chart"]))) {
            throw new Error('Chart is not defined! Please install the Chartjs library first.');
        }

        Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
        Chart.defaults.global.defaultFontColor = '#858796';

        const ctx = document.querySelector(selector);
        if (!ctx) throw new Error(`Could not find any DOM element matching the selector ${selector}.`);

        config.data || (config.data = {
            labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
            datasets: [{
                label: '# of Votes',
                data: [12, 19, 3, 5, 2, 3],
                borderWidth: 1
            }]
        });

        config.options || (config.options = {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        });

        console.log('Creating new chart for selector, and config:', selector, config);
        const chart = new Chart(ctx, config);

        return chart;
    }
}

export interface ChartjsConfig {
    data?: any;
    options?: any;
}
