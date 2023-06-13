import { TickerPriceInfo } from "./app/svc/instrument-prices.service";

export const groupBy = <T, K extends keyof any>(arr: T[], key: (i: T) => K) =>
  arr.reduce((groups, item) => {
    (groups[key(item)] ||= []).push(item);
    return groups;
  }, {} as Record<K, T[]>);

export function objToMap(o: any): Map <string, number> {
  return new Map(Object.entries(o))
};

export function sumValues(tickerInfos: TickerPriceInfo[]) {
  return tickerInfos
    .map(x => x.Value)
    .reduce((sum, current) => (sum ?? 0) + (current ?? 0));
}
