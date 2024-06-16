export interface FooterLinks
{
    Links: FooterLink[];
}
export interface FooterLink
{
    Text: string;
    Url: string|null|undefined;
}