import { apiClient } from "@/lib/api-client";
import type { ApiResponse } from "@/lib/types";

export interface RentRollLineDto {
  leaseNumber: string;
  propertyName: string;
  unitNumber: string;
  tenantNameAr: string;
  startDate: string;
  endDate: string;
  annualRentAmount: number;
  paymentFrequency: string;
  nextDueDate?: string;
}

export interface SalesPipelineStageDto {
  stage: string;
  count: number;
  totalAskingValue: number;
}

export interface CommissionSummaryLineDto {
  agentId: string;
  agentNameAr: string;
  commissionsCount: number;
  pendingAmount: number;
  approvedAmount: number;
  paidAmount: number;
  totalNetAmount: number;
}

export interface MaintenanceSummaryLineDto {
  status: string;
  count: number;
  totalEstimatedCost: number;
  totalActualCost: number;
}

export interface OccupancyLineDto {
  propertyId: string;
  propertyName: string;
  totalUnits: number;
  rentedUnits: number;
  soldUnits: number;
  availableUnits: number;
  occupancyRate: number;
}

export type ReportKey = "rent-roll" | "sales-pipeline" | "commission-summary" | "maintenance-summary" | "occupancy";

export async function getReport<T>(key: ReportKey) {
  const { data } = await apiClient.get<ApiResponse<T[]>>(`/reports/${key}`);
  return data.data;
}

export async function exportReport(key: ReportKey) {
  const response = await apiClient.get(`/reports/${key}/export`, { responseType: "blob" });
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement("a");
  link.href = url;
  link.download = `${key}.csv`;
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
