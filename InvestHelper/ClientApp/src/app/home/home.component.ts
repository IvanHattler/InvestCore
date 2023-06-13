import { Component, Pipe, PipeTransform, TemplateRef, ViewChild } from '@angular/core';
import { InstrumentPricesService, InstrumentType, TickerPriceInfo } from '../svc/instrument-prices.service';
import {
  ChartComponent,
  ApexNonAxisChartSeries,
  ApexResponsive,
  ApexChart
  } from "ng-apexcharts";
import { groupBy, objToMap, sumValues } from '../../helpers';
import { MatDialog } from '@angular/material/dialog';

export type ChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  responsive: ApexResponsive[];
  labels: any;
};

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  @ViewChild("instrumentsChart") instrumentsChart: ChartComponent = {} as ChartComponent;
  @ViewChild("percentsChart") percentsChart: ChartComponent = {} as ChartComponent;
  public instrumentsChartOptions: Partial<ChartOptions> | any;
  public percentsChartOptions: Partial<ChartOptions> | any;

  @ViewChild('dialogRef')
  dialogRef!: TemplateRef<any>;

  InstrumentType: typeof InstrumentType = InstrumentType;

  public tickerInfos: TickerPriceInfo[] = [];

  public overall: number | undefined;

  constructor(private instrumentPricesService: InstrumentPricesService,
    public dialog: MatDialog) {

    instrumentPricesService.getAll().subscribe(
      data => {
        this.tickerInfos = data;
        this.updateInstrumentChart();
        this.updatePercentsChart();
        this.updateOverall();
      },
      error => alert(error)
    );

    this.instrumentsChartOptions = {
      chart: {
        width: 500,
        type: "pie"
      },
      responsive: [
        {
          breakpoint: 480,
          options: {
            chart: {
              width: 200
            },
            legend: {
              position: "bottom"
            }
          }
        }
      ]
    };

    this.percentsChartOptions = {
      chart: {
        width: 400,
        type: "pie"
      },
      responsive: [
        {
          breakpoint: 480,
          options: {
            chart: {
              width: 200
            },
            legend: {
              position: "bottom"
            }
          }
        }
      ]
    };
  }

  showDialog() {
    let tickerInfo = new TickerPriceInfo(InstrumentType.Share, "", "", 0);
    const dialog = this.dialog.open(this.dialogRef, { data: tickerInfo });
    dialog.afterClosed().subscribe((res) => {
      if (res == "save") {
        this.tickerInfos.push(tickerInfo);
      }
    });
  }

  public loadPrices() {
    this.instrumentPricesService.getPrices(this.tickerInfos).subscribe(
      data => {
        let tickerPrices = objToMap(data);
        let t = this;
        tickerPrices.forEach(function (value, key) {
          let tickerInfo = t.tickerInfos.find(x => x.ticker == key);
          if (tickerInfo != null) {
            tickerInfo.price = value;
          }
        });

        this.updateInstrumentChart();
        this.updatePercentsChart();
        this.updateOverall();
      },
      err => {
        alert(err.message);
      });
  }

  private updatePercentsChart() {
      let percentsOfInstumentsGroups = Object.entries(groupBy(this.tickerInfos, i => InstrumentType[i.tickerType]));

      this.percentsChartOptions.labels = percentsOfInstumentsGroups
          .map(x => x[0]);
      this.percentsChartOptions.series = percentsOfInstumentsGroups
          .map(x => sumValues(x[1]));
  }

  private updateInstrumentChart() {
      this.instrumentsChartOptions.labels = [...this.tickerInfos.map(x => x.ticker)];
      this.instrumentsChartOptions.series = [...this.tickerInfos.map(x => x.value)];
  }

  private updateOverall() {
    this.overall = sumValues(this.tickerInfos);
  }

  public loadPrice(tickerInfo: TickerPriceInfo) {
    this.instrumentPricesService.getPrices([tickerInfo]).subscribe(
      data => {
        let tickerPrices = objToMap(data);
        let t = this;
        tickerPrices.forEach(function (value, key) {
          let tickerInfo = t.tickerInfos.find(x => x.ticker == key);
          if (tickerInfo != null) {
            tickerInfo.price = value;
          }
        });
      },
      err => {
        alert(err.message);
      });
  }

  public delete(tickerInfo: TickerPriceInfo) {
    const index = this.tickerInfos.indexOf(tickerInfo, 0);
    if (index > -1) {
      this.tickerInfos.splice(index, 1);
    }
  }
}
