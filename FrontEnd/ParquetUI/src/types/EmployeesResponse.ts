import type { RowData } from "@/types/RowData";

export interface EmployeesResponse {
    employees: RowData[];
    totalRecordCount: number;
    message: string;
}