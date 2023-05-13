import { TestBed } from '@angular/core/testing';

import { InstrumentPricesService } from './instrument-prices.service';

describe('InstrumentPricesService', () => {
  let service: InstrumentPricesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InstrumentPricesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
