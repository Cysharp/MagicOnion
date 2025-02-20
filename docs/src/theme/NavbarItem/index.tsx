import React, {type ReactNode} from 'react';
import NavbarItem from '@theme-original/NavbarItem';
import type NavbarItemType from '@theme/NavbarItem';
import type {WrapperProps} from '@docusaurus/types';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';

type Props = WrapperProps<typeof NavbarItemType> & { locale?: string[] };

export default function NavbarItemWrapper(props: Props): ReactNode {
  const ctx = useDocusaurusContext();

  if (props.locale != null && props.locale.indexOf(ctx.i18n.currentLocale) === -1) {
    return null;
  }
  return (
    <>
      <NavbarItem {...props} />
    </>
  );
}
