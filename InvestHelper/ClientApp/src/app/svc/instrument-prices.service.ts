import { Inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'InstrumentPrices/GetPrices';

export class TickerInfoBase {
  public TickerType: InstrumentType;
  public Ticker: string;
  public ClassCode: string;

  constructor(tickerType: InstrumentType, ticker: string, classCode: string) {
    this.TickerType = tickerType;
    this.Ticker = ticker;
    this.ClassCode = classCode;
  }
}

export class TickerPriceInfo extends TickerInfoBase {
  public Count: number | undefined;
  public Price: number | undefined;

  public get Value(): number | undefined {
    if (this.Count == null || this.Price == null) {
      return undefined;
    }
    return this.Count * this.Price;
  }

  constructor(tickerType: InstrumentType, ticker: string, classCode: string, count: number) {
    super(tickerType, ticker, classCode);
    this.Count = count;
  }
}

export enum InstrumentType {
  Share,
  Bond,
  Etf,
}

@Injectable({
  providedIn: 'root'
})
export class InstrumentPricesService {

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  public getPrices(tickerInfos: TickerInfoBase[]): Observable<Map<string, number>> {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };

    return this.http.post<Map<string, number>>(this.baseUrl + API_URL,
      JSON.stringify(tickerInfos),
      httpOptions);
  }
}
