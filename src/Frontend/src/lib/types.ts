export interface ApiResponse<T> {
  success: boolean;
  message: string | null;
  data: T;
  errors: string[] | null;
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface OwnerDto {
  id: string;
  ownerCode: string;
  partyType: number;
  nameAr: string;
  nameEn?: string;
  mobile: string;
  email?: string;
  city?: string;
  isActive: boolean;
  propertiesCount: number;
}

export interface PropertyDto {
  id: string;
  propertyCode: string;
  propertyName: string;
  propertyType: number;
  propertyCategory: number;
  status: number;
  ownerId: string;
  ownerNameAr: string;
  city?: string;
  district?: string;
  unitsCount: number;
  availableUnitsCount: number;
}

export interface UnitDto {
  id: string;
  unitCode: string;
  unitNumber: string;
  propertyId: string;
  propertyName: string;
  unitType: number;
  currentStatus: number;
  area?: number;
  rentalPrice?: number;
  salePrice?: number;
}

export interface AgreementDto {
  id: string;
  contractNumber: string;
  ownerId: string;
  ownerNameAr: string;
  propertyId: string;
  propertyName: string;
  startDate: string;
  endDate: string;
  status: number;
  daysRemaining: number;
}

export interface TenantDto {
  id: string;
  tenantCode: string;
  partyType: number;
  nameAr: string;
  nameEn?: string;
  mobile: string;
  email?: string;
  city?: string;
  employer?: string;
  isActive: boolean;
  leasesCount: number;
}

export interface LeaseDto {
  id: string;
  leaseNumber: string;
  tenantId: string;
  tenantNameAr: string;
  ownerId: string;
  ownerNameAr: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  startDate: string;
  endDate: string;
  annualRentAmount: number;
  paymentFrequency: number;
  numberOfPayments: number;
  status: number;
  daysRemaining: number;
}

export interface LeasePaymentDto {
  id: string;
  installmentNumber: number;
  dueDate: string;
  amount: number;
  paidAmount: number;
  remainingAmount: number;
  status: number;
  isOverdue: boolean;
}
