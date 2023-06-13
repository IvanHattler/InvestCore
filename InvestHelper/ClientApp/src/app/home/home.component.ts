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

  public tickerInfos: TickerPriceInfo[] = [
    new TickerPriceInfo(InstrumentType.Share, "SBER", "TQBR", 50),
    new TickerPriceInfo(InstrumentType.Share, "SNGSP", "TQBR", 700),
    new TickerPriceInfo(InstrumentType.Share, "MTSS", "TQBR", 100),
    new TickerPriceInfo(InstrumentType.Share, "MGNT", "TQBR", 2),
    new TickerPriceInfo(InstrumentType.Share, "OGKB", "TQBR", 40000),
    new TickerPriceInfo(InstrumentType.Share, "LKOH", "TQBR", 4),
    new TickerPriceInfo(InstrumentType.Share, "RASP", "TQBR", 70),
    new TickerPriceInfo(InstrumentType.Share, "MAGN", "TQBR", 400),
    new TickerPriceInfo(InstrumentType.Share, "WUSH", "TQBR", 5),
    new TickerPriceInfo(InstrumentType.Share, "PHOR", "TQBR", 2),
    new TickerPriceInfo(InstrumentType.Share, "MSFT", "SPBXM", 1),
    new TickerPriceInfo(InstrumentType.Share, "OZON", "TQBR", 2),
    new TickerPriceInfo(InstrumentType.Share, "INTC", "SPBXM", 5),
    new TickerPriceInfo(InstrumentType.Bond, "RU000A103WV8", "TQCB", 30),
    //new TickerPriceInfo(InstrumentType.Etf, "SBSP", "TQTF", 17),
    new TickerPriceInfo(InstrumentType.Etf, "SBMX", "TQTF", 6800),
    //new TickerPriceInfo(InstrumentType.Etf, "BOND", "TQTF", 3),
    //new TickerPriceInfo(InstrumentType.Etf, "SBGD", "TQTF", 1466),
    new TickerPriceInfo(InstrumentType.Etf, "SBGB", "TQTF", 4406),
  ];

  public overall: number | undefined;

  constructor(private instrumentPricesService: InstrumentPricesService,
              public dialog: MatDialog) {
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
          let tickerInfo = t.tickerInfos.find(x => x.Ticker == key);
          if (tickerInfo != null) {
            tickerInfo.Price = value;
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
      let percentsOfInstumentsGroups = Object.entries(groupBy(this.tickerInfos, i => InstrumentType[i.TickerType]));

      this.percentsChartOptions.labels = percentsOfInstumentsGroups
          .map(x => x[0]);
      this.percentsChartOptions.series = percentsOfInstumentsGroups
          .map(x => sumValues(x[1]));
  }

  private updateInstrumentChart() {
      this.instrumentsChartOptions.labels = [...this.tickerInfos.map(x => x.Ticker)];
      this.instrumentsChartOptions.series = [...this.tickerInfos.map(x => x.Value)];
  }

  private updateOverall() {
    this.overall = sumValues(this.tickerInfos);
  }

  //public loadPrice(tickerInfo: TickerPriceInfo) {
  //  this.instrumentPricesService.getPrices([tickerInfo]).subscribe(
  //    data => {
  //      let tickerPrices = objToMap(data);
  //      let t = this;
  //      tickerPrices.forEach(function (value, key) {
  //        let tickerInfo = t.tickerInfos.find(x => x.Ticker == key);
  //        if (tickerInfo != null) {
  //          tickerInfo.Price = value;
  //        }
  //      });
  //    },
  //    err => {
  //      alert(err.message);
  //    });
  //}
}

@Pipe({
  name: 'enumToArray'
})
export class EnumToArrayPipe implements PipeTransform {
  transform(data: Object) {
    const keys = Object.keys(data);
    return keys.slice(keys.length / 2);
  }
}
