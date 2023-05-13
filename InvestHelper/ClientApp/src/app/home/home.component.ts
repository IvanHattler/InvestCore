import { Component } from '@angular/core';
import { InstrumentPricesService, InstrumentType, ITickerInfoBase } from '../svc/instrument-prices.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  InstrumentType: typeof InstrumentType = InstrumentType;

  public tickerInfos: ITickerInfoBase[] = [
    <ITickerInfoBase>{
      Ticker: "SNGSP",
      TickerType: InstrumentType.Share,
      ClassCode: "TQBR",
    },
    <ITickerInfoBase>{
      Ticker: "SBER",
      TickerType: InstrumentType.Share,
      ClassCode: "TQBR",
    },
    <ITickerInfoBase>{
      Ticker: "SBMX",
      TickerType: InstrumentType.Etf,
      ClassCode: "TQTF",
    },
  ];

  public tickerPrices: Map<string, number> = new Map<string, number>();

  constructor(private instrumentPricesService: InstrumentPricesService) {
  }

  public loadPrices() {
    this.instrumentPricesService.getPrices(this.tickerInfos).subscribe(
      data => {
        this.tickerPrices = this.objToMap(data);
      },
      err => {
        alert(err.message);
      });
  }

  public loadPrice(tickerInfo: ITickerInfoBase) {
    this.instrumentPricesService.getPrices([tickerInfo]).subscribe(
      data => {
        const price = this.objToMap(data).get(tickerInfo.Ticker) ?? 0;
        this.tickerPrices.set(tickerInfo.Ticker, price);
      },
      err => {
        alert(err.message);
      });
  }

  private objToMap(o: any): Map<string, number> {
    return new Map(Object.entries(o))
  };
}
