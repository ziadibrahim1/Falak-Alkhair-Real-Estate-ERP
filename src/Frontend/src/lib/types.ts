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
  agentId?: string;
  agentNameAr?: string;
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

export interface AgentDto {
  id: string;
  agentCode: string;
  nameAr: string;
  nameEn?: string;
  mobile: string;
  email?: string;
  falLicenseNumber?: string;
  falLicenseExpiryDate?: string;
  specialization?: string;
  status: number;
  defaultCommissionPercentage: number;
  isActive: boolean;
  commissionsCount: number;
}

export interface BuyerDto {
  id: string;
  buyerCode: string;
  nameAr: string;
  nameEn?: string;
  mobile: string;
  email?: string;
  budget?: number;
  preferredCity?: string;
  preferredDistrict?: string;
  purpose: number;
  financingStatus: number;
  assignedAgentId?: string;
  assignedAgentNameAr?: string;
  isActive: boolean;
}

export interface PropertyMatchDto {
  propertyId: string;
  propertyCode: string;
  propertyName: string;
  city?: string;
  district?: string;
  unitId: string;
  unitCode: string;
  unitNumber: string;
  area?: number;
  salePrice?: number;
}

export interface SellerDto {
  id: string;
  sellerCode: string;
  ownerId: string;
  ownerNameAr: string;
  propertyId?: string;
  propertyName?: string;
  askingPrice: number;
  minimumPrice?: number;
  commissionPercentage: number;
  mandateStatus: number;
  mandateStartDate: string;
  mandateEndDate?: string;
  assignedAgentId?: string;
  assignedAgentNameAr?: string;
}

export interface LeadDto {
  id: string;
  leadCode: string;
  nameAr: string;
  mobile: string;
  email?: string;
  source: number;
  leadType: number;
  interestedPropertyId?: string;
  interestedPropertyName?: string;
  assignedAgentId?: string;
  assignedAgentNameAr?: string;
  status: number;
  priority: number;
}

export interface CommissionDto {
  id: string;
  commissionNumber: string;
  agentId: string;
  agentNameAr: string;
  sourceType: number;
  leaseId?: string;
  leaseNumber?: string;
  baseAmount: number;
  commissionPercentage: number;
  commissionAmount: number;
  vatAmount: number;
  netCommissionAmount: number;
  status: number;
}

export interface ListingDto {
  id: string;
  listingCode: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  listingType: number;
  price: number;
  description?: string;
  agentId?: string;
  agentNameAr?: string;
  listingStartDate?: string;
  listingEndDate?: string;
  status: number;
}

export interface MarketingCampaignDto {
  id: string;
  campaignCode: string;
  name: string;
  channel: number;
  startDate: string;
  endDate?: string;
  budget: number;
  actualCost: number;
  propertyId?: string;
  propertyName?: string;
  agentId?: string;
  agentNameAr?: string;
  isActive: boolean;
  leadsCount: number;
  conversionsCount: number;
}

export interface ViewingDto {
  id: string;
  viewingCode: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  buyerId?: string;
  buyerNameAr?: string;
  tenantId?: string;
  tenantNameAr?: string;
  agentId?: string;
  agentNameAr?: string;
  scheduledAt: string;
  status: number;
  feedback?: string;
}

export interface OfferDto {
  id: string;
  offerNumber: string;
  buyerId: string;
  buyerNameAr: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  amount: number;
  offerDate: string;
  expirationDate?: string;
  status: number;
}

export interface SaleDto {
  id: string;
  saleNumber: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  sellerId: string;
  sellerCode: string;
  buyerId: string;
  buyerNameAr: string;
  agentId?: string;
  agentNameAr?: string;
  askingPrice: number;
  finalPrice: number;
  commissionPercentage: number;
  stage: number;
  completedAt?: string;
}

export interface MaintenanceEmployeeDto {
  id: string;
  employeeCode: string;
  nameAr: string;
  mobile: string;
  department?: string;
  skills?: string;
  isAvailable: boolean;
  isActive: boolean;
  assignedRequestsCount: number;
}

export interface VendorDto {
  id: string;
  vendorCode: string;
  nameAr: string;
  contactPerson?: string;
  mobile: string;
  services?: string;
  rating?: number;
  isActive: boolean;
  assignedRequestsCount: number;
}

export interface MaintenanceRequestDto {
  id: string;
  requestNumber: string;
  propertyId: string;
  propertyName: string;
  unitId: string;
  unitNumber: string;
  requestType: number;
  priority: number;
  description: string;
  assignedEmployeeId?: string;
  assignedEmployeeNameAr?: string;
  assignedVendorId?: string;
  assignedVendorNameAr?: string;
  estimatedCost?: number;
  actualCost?: number;
  status: number;
}

export interface MaintenanceQuotationItemDto {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface MaintenanceQuotationDto {
  id: string;
  quotationNumber: string;
  vendorId: string;
  vendorNameAr: string;
  maintenanceRequestId: string;
  maintenanceRequestNumber: string;
  validUntil?: string;
  subtotalAmount: number;
  vatAmount: number;
  totalAmount: number;
  status: number;
  items: MaintenanceQuotationItemDto[];
}
